using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace OrderAPI.Repositories
{
    public class DynamoDBAdaptor
    {
        private readonly IDynamoDBContext _context;

        public DynamoDBAdaptor(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<Order> GetOrderByIdAsync(string orderId)
        {
            return await _context.LoadAsync<Order>(orderId);
        }

        public async Task SaveOrderAsync(Order order)
        {
            await _context.SaveAsync(order);
        }

        public async Task DeleteOrderAsync(string orderId)
        {
            await _context.DeleteAsync<Order>(orderId);
        }
    }
}
