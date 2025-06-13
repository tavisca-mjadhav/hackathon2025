using OrderAPI.Log;
using OrderAPI.Models;
using System.Text.Json;

public class PaymentService : IPaymentService
{
    private readonly CloudWatchLogger _cloudWatchLogs;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public PaymentService(CloudWatchLogger cloudWatchLogs, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _cloudWatchLogs = cloudWatchLogs;
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> ProcessPaymentAsync(PaymentRequest request)
    {
        var apiUrl = "http://54.84.244.6:8081/api/payment";
        var uri = new Uri(apiUrl);
        HttpContent rContent = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = rContent
        };
        string cid = _httpContextAccessor.HttpContext?.Request?.Headers["cid"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cid))
        {
            requestMessage.Headers.Add("cid", cid);
        }

        try
        {
            var response = await _httpClient.SendAsync(requestMessage);
            // response.EnsureSuccessStatusCode();

            // Optionally read response content
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return bool.Parse(content);
            }
            var result = JsonSerializer.Deserialize<Error>(content);
            throw result;
        }
        catch (Exception ex)
        {
            await _cloudWatchLogs.LogErrorAsync("Error", ex);
            return false;
        }
    }

}
