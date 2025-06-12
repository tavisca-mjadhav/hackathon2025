using Amazon.DynamoDBv2.Model;
using PaymentApi.Models;

namespace PaymentApi.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<GetItemResponse?> GetPaymentByIdAsync(string id);
        Task<bool> CreatePaymentAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int id);
    }
}
