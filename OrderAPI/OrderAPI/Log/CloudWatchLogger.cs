﻿using Amazon.CloudWatchLogs.Model;
using Amazon.CloudWatchLogs;
using System.Text;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace OrderAPI.Log
{
    public class CloudWatchLogger 
    {
        private readonly IAmazonCloudWatchLogs _cloudWatchLogs;
        private readonly CloudWatchLogContext _logContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _sequenceToken;

        public CloudWatchLogger(IAmazonCloudWatchLogs cloudWatchLogs, CloudWatchLogContext logContext, IHttpContextAccessor httpContextAccessor)
        {
            _cloudWatchLogs = cloudWatchLogs;
            _logContext = logContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogInfoAsync(string message, Object data = null)
        {
            await LogAsync("INFO", message, JsonSerializer.Serialize(data));
        }

        private string GetCid()
        {
            var ctx = _httpContextAccessor?.HttpContext;
            var cid = ctx?.Request?.Headers["cid"].FirstOrDefault();
            if (cid is null || cid.Equals(StringValues.Empty))
            {
                cid = Guid.NewGuid().ToString();
            }
            return cid;
        }

        public async Task LogErrorAsync(string message, Exception ex = null)
        {
            try
            {
                var test = new { ex.Message, ex.StackTrace };
                var error = JsonSerializer.Serialize(test);
                await LogAsync("ERROR", message, error);
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        private async Task LogAsync(string level, string message, string json)
        {
            var logBuilder = new StringBuilder();
            logBuilder.Append($"CID:{GetCid()}=>{DateTime.UtcNow:o} [appName:OrderAPI] level:{level}] [message:{message}] [data:{json}]");
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


