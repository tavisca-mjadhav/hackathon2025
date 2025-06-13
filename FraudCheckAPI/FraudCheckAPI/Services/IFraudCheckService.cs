using FraudCheckAPI.Log;
using FraudCheckAPI.Models;

namespace FraudCheckAPI.Services
{
    public interface IFraudCheckService
    {
        Task<FraudCheckResult> CheckFraudAsync(string orderId);
        Task<FraudCheckResponse> Check(FraudCheckRequest request);
    }
}
