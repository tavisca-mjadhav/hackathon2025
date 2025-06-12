
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Newtonsoft.Json;
using OrderAPI.Models;


public class PaymentService : IPaymentService
{
    private readonly IAmazonCloudWatchLogs _cloudWatchLogs;
    private readonly HttpClient _httpClient;
    public PaymentService(IAmazonCloudWatchLogs cloudWatchLogs)
    {
        _cloudWatchLogs = cloudWatchLogs;
    }

    public async Task<bool> ProcessPaymentAsync(PaymentRequest request)
    {
        var apiUrl = "https://external-payment-api.com/process";

        try
        {
            var response = await _httpClient.PostAsJsonAsync(apiUrl, request);
            response.EnsureSuccessStatusCode();

            // Optionally read response content
            var content = await response.Content.ReadAsStringAsync();

            return true;
        }
        catch (Exception ex)
        {
            LogExceptionToCloudWatch(request.OrderId, ex);
            return false;
        }
    }

    private async Task LogExceptionToCloudWatch(string orderId, Exception ex)
    {
        string logGroup = "PaymentErrors";
        string logStream = "ProcessPaymentErrors";

        try
        {
            await _cloudWatchLogs.CreateLogGroupAsync(new CreateLogGroupRequest { LogGroupName = logGroup });
        }
        catch (ResourceAlreadyExistsException) { }

        try
        {
            await _cloudWatchLogs.CreateLogStreamAsync(new CreateLogStreamRequest
            {
                LogGroupName = logGroup,
                LogStreamName = logStream
            });
        }
        catch (ResourceAlreadyExistsException) { }

        var logEvent = new InputLogEvent
        {
            Message = $"[{DateTime.UtcNow}] Error processing paynment for OrderId {orderId}: {ex}",
            Timestamp = DateTime.UtcNow
        };

        await _cloudWatchLogs.PutLogEventsAsync(new PutLogEventsRequest
        {
            LogGroupName = logGroup,
            LogStreamName = logStream,
            LogEvents = new List<InputLogEvent> { logEvent }
        });
    }


}
