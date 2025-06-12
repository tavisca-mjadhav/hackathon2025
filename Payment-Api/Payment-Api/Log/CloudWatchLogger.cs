using Amazon.CloudWatchLogs.Model;
using Amazon.CloudWatchLogs;
using PaymentApi.Interfaces;
using System.Text;

namespace PaymentApi.Log
{
    public class CloudWatchLogger: ICloudWatchLogger
    {
        private readonly IAmazonCloudWatchLogs _cloudWatchLogs;
        private readonly CloudWatchLogContext _logContext;
        private string _sequenceToken;

        public CloudWatchLogger(IAmazonCloudWatchLogs cloudWatchLogs, CloudWatchLogContext logContext)
        {
            _cloudWatchLogs = cloudWatchLogs;
            _logContext = logContext;
        }

        public async Task LogInfoAsync(string message, Dictionary<string, object> fields = null)
        {
            await LogAsync("INFO", message,fields);
        }

        public async Task LogErrorAsync(string message, Exception ex = null , Dictionary<string, object> fields = null)
        {
            var errorMessage = $"{message}{(ex != null ? $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}" : "")}";
           
            await LogAsync("ERROR", errorMessage);
        }

        private async Task LogAsync(string level, string message, Dictionary<string, object> fields = null)
        {
            var logBuilder = new StringBuilder();
            logBuilder.Append($"{DateTime.UtcNow:o} [{level}] {message}");
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    logBuilder.Append($" | {field.Key}: {field.Value}");
                }
            }
            var logEvent = new InputLogEvent
            {
                Message = logBuilder.ToString(),
                Timestamp = DateTime.UtcNow
            };

            var request = new PutLogEventsRequest
            {
                LogGroupName = _logContext.LogGroupName,
                LogStreamName = _logContext.LogStreamName,
                LogEvents = new List<InputLogEvent> { logEvent },
                SequenceToken = _sequenceToken
            };

            try
            {
                var response = await _cloudWatchLogs.PutLogEventsAsync(request);
                _sequenceToken = response.NextSequenceToken;
            }
            catch (InvalidSequenceTokenException ex)
            {
                _sequenceToken = ex.ExpectedSequenceToken;
                request.SequenceToken = _sequenceToken;
                await _cloudWatchLogs.PutLogEventsAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CloudWatch logging failed: {ex.Message}");
            }
        }
    }

}


