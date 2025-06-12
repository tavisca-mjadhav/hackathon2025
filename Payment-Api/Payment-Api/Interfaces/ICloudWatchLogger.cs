
namespace PaymentApi.Interfaces
{
    public interface ICloudWatchLogger
    {
        Task LogInfoAsync(string message, Object data = null);
        Task LogErrorAsync(string message, Exception ex = null);
    }
}
