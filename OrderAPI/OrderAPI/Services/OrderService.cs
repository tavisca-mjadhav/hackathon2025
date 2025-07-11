
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Newtonsoft.Json;
using OrderAPI.Models;
using OrderAPI.Translator;
using OrderAPI.Common;
using OrderAPI.Log;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using OrderAPI.Services;


public class OrderService : IOrderService
{
    private readonly CloudWatchLogger _cloudWatchLogs;
    private readonly IPaymentService _paymentService;
    private readonly AmazonClient _amazonClient;
    private readonly IFraudCheck _fraudCheck;


    public OrderService(AmazonClient amazonClient, CloudWatchLogger cloudWatchLogs, IPaymentService paymentService, IFraudCheck fraudCheck)
    {
        _amazonClient = amazonClient;
        _cloudWatchLogs = cloudWatchLogs;
        _paymentService = paymentService;
        _fraudCheck = fraudCheck;
    }

    public async Task<Order> CreateOrderAsync(OrderRequest orderRequest)
    {
        //await LogToCloudWatch("Create Order Process Started");
        var order = new Order();
        try
        {
            await _cloudWatchLogs.LogInfoAsync($"started Order api", orderRequest);
            //var table = Table.LoadTable(_dynamoDb, "nextgen_order");
            var request = new FraudCheckRequest { Amount = orderRequest.Price, CardNumber = orderRequest.Card.Number, IPAddress = "123.2356" };
            var fraudCheckResult = await _fraudCheck.Check(request);
            if (!fraudCheckResult.isFraud)
            {
                order = orderRequest.ToOrder();
                var response = await _amazonClient.PutItem(order);


                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    //payment call
                    var paymentRequest = GetPaymentRequest(order, orderRequest);
                    var status = await _paymentService.ProcessPaymentAsync(paymentRequest);
                    order.PaymentStatus = status ? PaymentStatus.FullyPaid.ToString() : PaymentStatus.Failed.ToString();
                    response = await _amazonClient.PutItem(order);
                }
                // Update item in DynamoDB with new status
                //doc = Document.FromJson(JsonConvert.SerializeObject(order));
                //await table.PutItemAsync(doc);  // This replaces the existing item

                await _cloudWatchLogs.LogInfoAsync($"Order saved successfully with ID {order.OrderId}");
                return order;
            }
        }
        catch (Exception ex)
        {
            await LogExceptionToCloudWatch(order.OrderId, ex);
        }
        return null;

    }

    private static PaymentRequest GetPaymentRequest(Order order, OrderRequest orderRequest)
    {
        return new PaymentRequest
        {
            OrderId = order.OrderId,
            Amount = order.Price,
            Currency = "USD",
            PaymentId = CommonUtils.GenerateRandomAlphanumeric(),
            PaymentType = "Card",
            Card = new Card
            {
                Number = orderRequest.Card.Number, // Example card number
                CVV = orderRequest.Card.CVV, // Example CVV
                Expiry = new CardExpiry() { Month = orderRequest.Card.Expiry.Month, Year = orderRequest.Card.Expiry.Year }, // Example expiry date
                HolderName = orderRequest.Card.HolderName ,// Example name on card
                IssuedBy = orderRequest.Card.IssuedBy
            }

        };
    }

    public async Task<Order> GetOrderByIdAsync(string orderId)
    {
        try
        {
            //var table = Table.LoadTable(_dynamoDb, "Orders");
            //var document = await table.GetItemAsync(orderId);
            //if (document == null) return null;

            // return JsonConvert.DeserializeObject<Order>(document.ToJson());

        }
        catch (Exception ex)
        {
            await LogExceptionToCloudWatch(orderId, ex);
        }
        return null;
    }

    private async Task LogToCloudWatch(string message)
    {
        try
        {
            var logGroup = "PaymentApiLogs";
            //var logStream = "OrderStream";

            //await _cloudWatchLogs.CreateLogGroupAsync(new CreateLogGroupRequest { LogGroupName = logGroup });
            //await _cloudWatchLogs.CreateLogStreamAsync(new CreateLogStreamRequest { LogGroupName = logGroup, LogStreamName = logStream });

            var logEvents = new List<InputLogEvent>
        {
            new InputLogEvent
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            }
        };

            //await _cloudWatchLogs.PutLogEventsAsync(new PutLogEventsRequest
            //{
            //    LogGroupName = logGroup,
            //    //LogStreamName = logStream,
            //    LogEvents = logEvents
            //});
        }
        catch (Exception ex)
        {
            var data = ex.Message;
            throw;
        }

    }

    private async Task LogExceptionToCloudWatch(string orderId, Exception ex)
    {
        string logGroup = "OrderErrors";
        string logStream = "GetOrderErrors";

        //try
        //{
        //    await _cloudWatchLogs.CreateLogGroupAsync(new CreateLogGroupRequest { LogGroupName = logGroup });
        //}
        //catch (ResourceAlreadyExistsException) { }

        //try
        //{
        //    await _cloudWatchLogs.CreateLogStreamAsync(new CreateLogStreamRequest
        //    {
        //        LogGroupName = logGroup,
        //        LogStreamName = logStream
        //    });
        //}
        //catch (ResourceAlreadyExistsException) { }

        var logEvent = new InputLogEvent
        {
            Message = $"[{DateTime.UtcNow}] Error retrieving OrderId {orderId}: {ex}",
            Timestamp = DateTime.UtcNow
        };

        //await _cloudWatchLogs.PutLogEventsAsync(new PutLogEventsRequest
        //{
        //    LogGroupName = logGroup,
        //    LogStreamName = logStream,
        //    LogEvents = new List<InputLogEvent> { logEvent }
        //});
    }
}
