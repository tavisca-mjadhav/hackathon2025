using Amazon.CloudWatchLogs.Model;
using Amazon.CloudWatchLogs;
using Amazon;
using System.Text;

namespace LLM_app
{
    internal class CloudWatchService
    {
        private readonly IAmazonCloudWatchLogs _cloudWatchLogsClient;

        public CloudWatchService(string awsAccessKey, string awsSecretKey, string region)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            _cloudWatchLogsClient = new AmazonCloudWatchLogsClient(credentials, RegionEndpoint.GetBySystemName(region));
        }

        public async Task<List<FilterLogEventsResponse>> FetchLogsForLastHour(string logGroupName)
        {
            var logs = new List<FilterLogEventsResponse>();
            try
            {
                var endTime = DateTime.UtcNow;
                var startTime = endTime.AddHours(-1);

                Console.WriteLine($"Fetching logs from {startTime} to {endTime}");
                Console.WriteLine($"Log Group: {logGroupName}");
                Console.WriteLine("----------------------------------------");

                var request = new FilterLogEventsRequest
                {
                    LogGroupName = logGroupName,
                    StartTime = new DateTimeOffset(startTime).ToUnixTimeMilliseconds(),
                    EndTime = new DateTimeOffset(endTime).ToUnixTimeMilliseconds(),
                    Limit = 50  // Adjust this value based on your needs
                };

                string nextToken = null;
                int totalEvents = 0;

                do
                {
                    request.NextToken = nextToken;
                    var response = await _cloudWatchLogsClient.FilterLogEventsAsync(request);

                    foreach (var evt in response.Events)
                    {
                        totalEvents++;
                        Console.WriteLine($"Timestamp: {evt.Timestamp}");
                        Console.WriteLine($"Stream: {evt.LogStreamName}");
                        Console.WriteLine($"Message: {evt.Message}");
                        Console.WriteLine("----------------------------------------");
                    }
                    logs.Add(response);
                    nextToken = response.NextToken;

                } while (nextToken != null);

                Console.WriteLine($"Total events found: {totalEvents}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching logs: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
            return logs;
        }

        // Optional: Method to fetch logs with a specific filter pattern
        public async Task<string> FetchFilteredLogs(string logGroupName, string filterPattern)
        {
            try
            {
                StringBuilder finalResponse = new StringBuilder();
                var endTime = DateTime.UtcNow;
                var startTime = endTime.AddHours(-1);

                Console.WriteLine($"Fetching filtered logs from {startTime} to {endTime}");
                Console.WriteLine($"Log Group: {logGroupName}");
                Console.WriteLine($"Filter Pattern: {filterPattern}");
                Console.WriteLine("----------------------------------------");

                var request = new FilterLogEventsRequest
                {
                    LogGroupName = logGroupName,
                    StartTime = new DateTimeOffset(startTime).ToUnixTimeMilliseconds(),
                    EndTime = new DateTimeOffset(endTime).ToUnixTimeMilliseconds(),
                    FilterPattern = filterPattern,
                    Limit = 50
                };

                string nextToken = null;
                int totalEvents = 0;

                do
                {
                    request.NextToken = nextToken;
                    var response = await _cloudWatchLogsClient.FilterLogEventsAsync(request);
                    
                    foreach (var evt in response.Events)
                    {
                        totalEvents++;
                        finalResponse.AppendLine($"Timestamp: {evt.Timestamp}");
                        finalResponse.AppendLine($"Stream: {evt.LogStreamName}");
                        finalResponse.AppendLine($"Message: {evt.Message}");
                        finalResponse.AppendLine("----------------------------------------");
                        //finalResponse.AppendLine(evt.ToString());
                    }

                    nextToken = response.NextToken;

                } while (nextToken != null);

                Console.WriteLine($"Total filtered events found: {totalEvents}");
                return finalResponse.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching filtered logs: {ex.Message}");
                return null;
            }
        }
    }
}
