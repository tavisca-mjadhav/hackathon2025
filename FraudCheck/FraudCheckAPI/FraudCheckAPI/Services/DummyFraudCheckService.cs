using FraudCheckAPI.Log;
using FraudCheckAPI.Models;

namespace FraudCheckAPI.Services
{
    public class DummyFraudCheckService : IFraudCheckService
    {
        private static readonly Random _random = new Random();
        private readonly List<string> blacklistedCards = new() { "1111222233334444", "0000111122223333" };
        private readonly CloudWatchLogger _cloudWatchLogs;
        public DummyFraudCheckService(CloudWatchLogger cloudWatchLogger)
        {
            _cloudWatchLogs = cloudWatchLogger;
        }

        public Task<FraudCheckResult> CheckFraudAsync(string orderId)
        {
            Task.Delay(100).Wait();
            bool isFraud = _random.Next(0, 10) < 2;

            var result = new FraudCheckResult
            {
                OrderId = orderId,
                IsFraudulent = isFraud,
                Reason = isFraud ? "Suspicious transaction pattern detected." : "No issues found."
            };

            return Task.FromResult(result);
        }

        public async Task<FraudCheckResponse> Check(FraudCheckRequest request)
        {
            try
            {
                var response = new FraudCheckResponse();
                request.CardNumber.Substring(1, 5);
                if (blacklistedCards.Contains(request.CardNumber))
                {
                    response.IsFraud = true;
                    response.Reasons = "Card is blacklisted.";
                }
                var value = 10 / request.Amount;
                if (request.Amount > 10000)
                {
                    response.IsFraud = true;
                    response.Reasons = "High payment amount flagged.";
                }

                if (request.IPAddress.StartsWith("192.168"))
                {
                    response.IsFraud = true;
                    response.Reasons = "IP from suspicious range.";
                }
                return response;
            }
            catch (Exception ex)
            {
                _cloudWatchLogs.LogErrorAsync("Error during fraud check", ex);
                return new FraudCheckResponse
                {
                    IsFraud = true,
                    Reasons = "An error occurred during fraud check. " + "Message:" + ex.Message
                };

            }


        }
    }
}




