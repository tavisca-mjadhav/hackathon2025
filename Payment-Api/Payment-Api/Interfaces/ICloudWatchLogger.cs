
namespace PaymentApi.Interfaces
{
    public interface ICloudWatchLogger
    {
        Task LogInfoAsync(string message, Dictionary<string, object> fields = null);
        Task LogErrorAsync(string message, Exception ex = null, Dictionary<string, object> fields = null);
     }
}
