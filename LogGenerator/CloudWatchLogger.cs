namespace LogGenerator
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Amazon.CloudWatchLogs.Model;
    using Amazon.CloudWatchLogs;
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using Amazon;
    using Amazon.Runtime;
    using System.Net.Http;

    public class CloudWatchLogger
    {
        private readonly string _logGroupName;
        private readonly string _logStreamName;
        private readonly IAmazonCloudWatchLogs _client;
        private string _sequenceToken;

        public CloudWatchLogger(string logGroupName, string logStreamName, string credsUrl)
        {
            _logGroupName = logGroupName;
            _logStreamName = logStreamName;

            var credentials = GetCredentialsFromUrlAsync(credsUrl).GetAwaiter().GetResult();
            var awsCredentials = new BasicAWSCredentials(credentials.AccessKeyId, credentials.SecretAccessKey);
            var region = RegionEndpoint.GetBySystemName(credentials.Region);

            _client = new AmazonCloudWatchLogsClient(awsCredentials, region);

            // Initialize log group and stream
            InitializeAsync().GetAwaiter().GetResult();
        }



        public async Task WriteLogAsync(object structuredLog)
        {
            var message = JsonSerializer.Serialize(structuredLog);

            Console.WriteLine($"Writing log to CloudWatch: {message}");

            var logEvent = new InputLogEvent
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            var request = new PutLogEventsRequest
            {
                LogGroupName = _logGroupName,
                LogStreamName = _logStreamName,
                LogEvents = new List<InputLogEvent> { logEvent },
                SequenceToken = _sequenceToken
            };

            try
            {
                var response = await _client.PutLogEventsAsync(request);
                _sequenceToken = response.NextSequenceToken;
            }
            catch (InvalidSequenceTokenException ex)
            {
                _sequenceToken = ex.ExpectedSequenceToken;
                request.SequenceToken = _sequenceToken;
                var retryResponse = await _client.PutLogEventsAsync(request);
                _sequenceToken = retryResponse.NextSequenceToken;
            }
        }

        private async Task InitializeAsync()
        {
            await EnsureLogGroupAsync();
            await EnsureLogStreamAsync();
        }

        private async Task EnsureLogGroupAsync()
        {
            var groups = await _client.DescribeLogGroupsAsync(new DescribeLogGroupsRequest
            {
                LogGroupNamePrefix = _logGroupName
            });

            if (!groups.LogGroups.Any(g => g.LogGroupName == _logGroupName))
            {
                await _client.CreateLogGroupAsync(new CreateLogGroupRequest
                {
                    LogGroupName = _logGroupName
                });
            }
        }

        private async Task EnsureLogStreamAsync()
        {
            var streams = await _client.DescribeLogStreamsAsync(new DescribeLogStreamsRequest
            {
                LogGroupName = _logGroupName,
                LogStreamNamePrefix = _logStreamName
            });

            var stream = streams.LogStreams.FirstOrDefault(s => s.LogStreamName == _logStreamName);
            if (stream == null)
            {
                await _client.CreateLogStreamAsync(new CreateLogStreamRequest
                {
                    LogGroupName = _logGroupName,
                    LogStreamName = _logStreamName
                });
            }
            else
            {
                _sequenceToken = stream.UploadSequenceToken;
            }
        }

        private async Task<(string AccessKeyId, string SecretAccessKey, string Region)> GetCredentialsFromUrlAsync(string url)
        {
            using var http = new HttpClient();
            var json = await http.GetStringAsync(url);

            var doc = JsonDocument.Parse(json);
            return (
                AccessKeyId: doc.RootElement.GetProperty("keyId").GetString(),
                SecretAccessKey: doc.RootElement.GetProperty("secretKey").GetString(),
                Region: json.Contains("region")
                    ? doc.RootElement.GetProperty("region").GetString()
                    : "us-east-1" // Default region if not specified
            );
        }
    }


}
