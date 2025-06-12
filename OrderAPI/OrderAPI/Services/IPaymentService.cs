
using OrderAPI.Models;
using System.Threading.Tasks;


public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(PaymentRequest request);
}
