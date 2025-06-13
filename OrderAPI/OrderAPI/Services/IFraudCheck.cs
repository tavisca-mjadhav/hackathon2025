namespace OrderAPI.Services
{
    public interface IFraudCheck
    {
        Task<bool> HealthCheck(bool isFaultInjection);
    }
}
