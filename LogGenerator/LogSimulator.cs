namespace LogGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection.Emit;
    using System.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class CategorizedException
    {
        public Exception Exception { get; set; }
        public string Category { get; set; } // "technical", "business", "infrastructure"
        public bool ShouldRetry { get; set; }
    }

    public class SimulationRunner
    {
        private readonly string[] _services = new[]
        {
           "OrderService", "PaymentService", "InventoryService", "AuthService", "ShippingService", "CatalogService", "ChargeService"
        };

        private readonly Random _rand = new Random();
        private readonly CloudWatchLogger _cloudLogger;

        public SimulationRunner(CloudWatchLogger cloudLogger)
        {
            _cloudLogger = cloudLogger;
        }

        public async Task RunSimulationAsync()
        {
            var faker = new Bogus.Faker();

            while (true)
            {
                var cid = Guid.NewGuid().ToString(); // shared across services in a request

                foreach (var service in _services.OrderBy(_ => _rand.Next()))
                {
                    var isError = _rand.NextDouble() < 0.8;
                    var latency = _rand.Next(30, 1500);
                    var requestId = Guid.NewGuid().ToString();
                    var userAgent = faker.Internet.UserAgent();

                    var retryAttempt = 0;
                    var maxRetry = 3;

                    if (isError)
                    {
                        var errorResult = GenerateRandomException();
                        var ex = errorResult.Exception;

                        do
                        {
                            await _cloudLogger.WriteLogAsync(new
                            {
                                cid,
                                requestId,
                                service,
                                eventType = "Error",
                                level = "ERROR",
                                category = errorResult.Category,
                                exception = ex.GetType().Name,
                                message = ex.Message,
                                stackTrace = ex.StackTrace ?? "Stack trace not available.",
                                retryAttempt,
                                willRetry = errorResult.ShouldRetry && retryAttempt < maxRetry,
                                latencyMs = latency,
                                userAgent = faker.Internet.UserAgent(),
                                timestamp = DateTime.UtcNow,
                                downstreamService = errorResult.Exception is ExternalServiceException esc ? esc.ServiceName : null,
                                downstreamStatus = errorResult.Exception is ExternalServiceException esc1 ? esc1.StatusCode : default,
                            });

                            if (!errorResult.ShouldRetry || retryAttempt >= maxRetry)
                                break;

                            retryAttempt++;
                            await Task.Delay(300 + _rand.Next(200)); // backoff

                        } while (true);
                    }
                    else
                    {
                        // Simulate a DB call
                        await LogEventAsync("INFO", "DbCall", cid, requestId, service, new
                        {
                            query = $"SELECT * FROM {service.Replace("Service", "")}s WHERE Id = '{Guid.NewGuid()}'",
                            dbHost = faker.Internet.Ip(),
                            dbLatencyMs = _rand.Next(10, 250)
                        });
                        object context = service switch
                        {
                            "PaymentService" => new
                            {
                                amount = faker.Finance.Amount(10, 1000),
                                currency = faker.Finance.Currency().Code,
                                paymentMethod = faker.PickRandom("CreditCard", "UPI", "PayPal", "NetBanking")
                            },
                            "ChargeService" => new
                            {
                                amount = faker.Finance.Amount(10, 1000),
                                currency = faker.Finance.Currency().Code,
                                paymentMethod = faker.PickRandom("CASH", "POINTS")
                            },
                            "OrderService" => new
                            {
                                orderId = faker.Random.Guid(),
                                itemsCount = faker.Random.Int(1, 10),
                                totalPrice = faker.Finance.Amount(50, 2000)
                            },
                            "InventoryService" => new
                            {
                                productId = faker.Commerce.Ean13(),
                                stockLevel = faker.Random.Int(0, 1000),
                                warehouseId = faker.Address.ZipCode()
                            },
                            "AuthService" => new
                            {
                                userId = faker.Internet.UserName(),
                                authType = faker.PickRandom("OAuth", "Password", "Token"),
                                ipAddress = faker.Internet.Ip()
                            },
                            "ShippingService" => new
                            {
                                shipmentId = faker.Random.Guid(),
                                carrier = faker.PickRandom("FedEx", "DHL", "UPS", "BlueDart"),
                                estimatedDelivery = faker.Date.Future().ToString("yyyy-MM-dd")
                            },
                            "CatalogService" => new
                            {
                                productId = faker.Commerce.Ean13(),
                                category = faker.Commerce.Categories(1).FirstOrDefault(),
                                price = faker.Commerce.Price()
                            },
                            _ => new { }
                        };
                        await LogEventAsync("INFO", "EndRequest", cid, requestId, service, context);
                        await _cloudLogger.WriteLogAsync(new
                        {
                            cid,
                            requestId,
                            service,
                            level = "INFO",
                            message = $"Successfully processed {service} request.",
                            context,
                            latencyMs = latency,
                            userAgent,
                            timestamp = DateTime.UtcNow
                        });
                    }

                    await Task.Delay(_rand.Next(100, 300));
                }

                await Task.Delay(1000);
            }
        }

        private async Task LogEventAsync(string level, string eventType, string cid, string requestId, string service, object? context = null)
        {
            var faker = new Bogus.Faker();

            var log = new Dictionary<string, object>
            {
                // Correlation
                ["cid"] = cid,
                ["requestId"] = requestId,
                ["eventType"] = eventType,
                ["service"] = service,
                ["level"] = level,

                // Latency and metadata
                ["latencyMs"] = _rand.Next(20, 500),
                ["userAgent"] = faker.Internet.UserAgent(),
                ["timestamp"] = DateTime.UtcNow.ToString("o"),

                // Enrichment
                ["traceId"] = Guid.NewGuid().ToString(),
                ["region"] = faker.PickRandom("us-east-1", "us-west-2", "eu-central-1", "ap-south-1"),
                ["containerId"] = $"container-{_rand.Next(1000, 9999)}",
                ["host"] = Environment.MachineName
            };

            if (context != null)
                log["context"] = context;

            await _cloudLogger.WriteLogAsync(log);
        }



        private CategorizedException GenerateRandomException()
        {
            var faker = new Bogus.Faker();

            var exceptionOptions = new List<(Func<Exception> Generator, string Category, bool ShouldRetry)>
    {
        // Technical
        (() => throw new TimeoutException("The operation timed out while waiting for response."), "technical", true),
        (() => throw new InvalidOperationException("Operation is not valid in current state."), "technical", false),
        (() => throw new NullReferenceException("Object reference not set to an instance of an object."), "technical", false),
        (() => throw new FormatException("The input format was invalid."), "technical", false),
        (() => throw new DivideByZeroException("Attempted to divide by zero."), "technical", false),
        (() => throw new IndexOutOfRangeException("Index exceeded array bounds."), "technical", false),
        (() => throw new StackOverflowException("Too many recursive calls."), "technical", false),
        (() => throw new HttpRequestException("HTTP request failed due to unreachable endpoint."), "technical", true),
        (() => throw new OperationCanceledException("The operation was canceled due to timeout."), "technical", true),
        (() => throw new TaskCanceledException("Task was canceled before completion."), "technical", true),

        // Business
        (() => throw new ApplicationException("Business rule violated: cannot refund more than amount paid."), "business", false),
        (() => throw new InvalidOperationException("Uploaded file format not allowed."), "business", false),
        (() => throw new UnauthorizedAccessException("User does not have permission for this operation."), "business", false),
        (() => throw new ArgumentOutOfRangeException("Discount cannot exceed 100%."), "business", false),
        (() => throw new ArgumentException("Required parameter 'userId' was not supplied."), "business", false),
        (() => throw new ArgumentException("Model validation failed for input request."), "business", false),
        (() => throw new InvalidOperationException("Order cannot be cancelled after shipment."), "business", false),
        (() => throw new NotSupportedException("Feature not supported in current subscription plan."), "business", false),

        // Infrastructure
        (() => throw new IOException("Failed to write log file due to disk I/O error."), "infrastructure", true),
        (() => throw new Exception("SqlException: Deadlock occurred during transaction."), "infrastructure", true),
        (() => throw new Exception("RedisConnectionException: Unable to connect to Redis cluster."), "infrastructure", true),
        (() => throw new Exception("KafkaException: Leader not available."), "infrastructure", true),
        (() => throw new Exception("ElasticSearchTimeout: Query took too long."), "infrastructure", true),
        (() => throw new Exception("StorageQuotaExceededException: File system limit reached."), "infrastructure", true),
        (() => throw new Exception("CircuitBreakerOpenException: Downstream service call blocked."), "infrastructure", false),
        (() => throw new Exception("NetworkException: DNS lookup failed."), "infrastructure", true),

        // Security
        (() => throw new SecurityException("Access token expired."), "security", false),
        (() => throw new AuthenticationException("Invalid credentials provided."), "security", false),
        (() => throw new CryptographicException("Unable to decrypt payment token."), "security", false),

        // Serialization & Messaging
        (() => throw new JsonException("Failed to parse JSON payload from upstream system."), "technical", false),
        (() => throw new InvalidCastException("Unable to cast object to required type."), "technical", false),
        (() => throw new Exception("MessageQueueException: Unable to enqueue message."), "infrastructure", true),
        (() => throw new Exception("BlobStorageException: Container does not exist."), "infrastructure", true),



        // Nested exception simulating downstream call
        (() => new ExternalServiceException(
                "Failed to call OrderService",
                504,
                "OrderService",
                new TimeoutException("OrderService did not respond in time.")),
            "infrastructure", true),

        // Payment failure
        (() => new PaymentDeclinedException("Insufficient funds", "PAYMENT-DECLINE-001"), "business", false),

        // Gateway timeout
        (() => new ExternalServiceException("Gateway Timeout from payment gateway", 504, "PaymentGateway"),
            "infrastructure", true),

        // Throttling
        (() => new ExternalServiceException("Too many requests", 429, "UserService"),
            "infrastructure", true),

        // Forbidden
        (() => new ExternalServiceException("Access forbidden to resource", 403, "InvoiceAPI"),
            "security", false),

        // Authentication failure with error code
        (() => new AuthenticationException("Invalid API token: AUTH-FAIL"), "security", false),

        // SQL error with error code
        (() => new Exception("SqlException: Deadlock encountered [ERR-DB-DEADLOCK]"), "infrastructure", true),

        // Basic exceptions for variety
        (() => new NullReferenceException("Null object encountered in pipeline."), "technical", false),
        (() => new ArgumentException("Missing required argument 'userId'."), "business", false),
        (() => new IOException("Disk write failure [ERR-DISK-FULL]"), "infrastructure", true),
        (() => new JsonException("Failed to parse upstream response [ERR-JSON-INVALID]"), "technical", false),
        (() => new Exception("Unknown error occurred [ERR-UNKNOWN]"), "technical", false),
    };

            var (thrower, category, shouldRetry) = faker.PickRandom(exceptionOptions);

            // Instead of wrapping in try/catch, just throw and catch naturally to preserve downstream context
            Exception finalEx;

            try
            {
                throw thrower.Invoke();
            }
            catch (ExternalServiceException ex)
            {
                // Downstream failure: preserve as-is
                finalEx = ex;
            }
            catch (Exception ex)
            {
                // Everything else: return original exception
                finalEx = ex;
            }

            return new CategorizedException
            {
                Exception = finalEx,
                Category = category,
                ShouldRetry = shouldRetry
            };
            return GenerateRandomException();
        }


    }

    public class PaymentDeclinedException : Exception
    {
        public string DeclineReason { get; }
        public string ErrorCode { get; }

        public PaymentDeclinedException(string reason, string code)
            : base($"Payment declined: {reason}")
        {
            DeclineReason = reason;
            ErrorCode = code;
        }
    }

    public class ExternalServiceException : Exception
    {
        public int StatusCode { get; }
        public string ServiceName { get; }

        public ExternalServiceException(string message, int statusCode, string serviceName, Exception? inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
            ServiceName = serviceName;
        }
    }

}
