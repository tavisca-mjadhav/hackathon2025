using FraudCheckAPI.Models;

namespace FraudCheckAPI.Services
{
    public class DummyFraudCheckService : IFraudCheckService
    {
        private static readonly Random _random = new Random();

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

    }
}
