
using OrderAPI.Models;
using System.Threading.Tasks;


public interface IOrderService
{
    Task<Order> CreateOrderAsync(OrderRequest orderId);

    Task<Order> GetOrderByIdAsync(string orderId);
}
