using Amazon.Runtime.Internal;
using OrderAPI.Log;
using System.Text.Json;

namespace OrderAPI.Services
{
    public class FraudCheck : IFraudCheck
    {
        private readonly HttpClient _httpClient;
        private readonly CloudWatchLogger _cloudWatchLogs;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public FraudCheck(CloudWatchLogger cloudWatchLogs, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _cloudWatchLogs = cloudWatchLogs;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HealthCheck(bool isFaultInjection)
        {
            var apiUrl = $"https://localhost:7217/api/Fraud/getIsFault/{isFaultInjection}";
            var uri = new Uri(apiUrl);

            // Create GET request without body
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            // Forward the `cid` header if available
            var cid = _httpContextAccessor.HttpContext?.Request?.Headers["cid"].FirstOrDefault();
            if (!string.IsNullOrEmpty(cid))
            {
                requestMessage.Headers.Add("cid", cid);
            }


            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    await _cloudWatchLogs.LogErrorAsync(
                        "FraudService: Failed GET call to fraud API",
                        new Exception($"StatusCode: {response.StatusCode}, Body: {responseContent}")
                    );
                    return false;
                }

                // If response is something like true/false as string
                return true;
            }
            catch (Exception ex)
            {
                await _cloudWatchLogs.LogErrorAsync("FraudService: Exception during GET call", ex);
                return false;
            }
        }
    }
}
