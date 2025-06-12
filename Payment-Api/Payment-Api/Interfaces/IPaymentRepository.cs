using Amazon.DynamoDBv2.Model;
using PaymentApi.Models;

namespace PaymentApi.Interfaces
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<GetItemResponse> GetByIdAsync(string id);
        Task<bool> AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(int id);
    }
}
