using Amazon.DynamoDBv2.Model;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Log;
using PaymentApi.Interfaces;
using PaymentApi.Models;

namespace PaymentApi.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;
        private readonly AmazonClient _amazonClient;

        public PaymentRepository(PaymentDbContext context , AmazonClient amazonClient)
        {
            _context = context;
            _amazonClient = amazonClient;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        public async Task<GetItemResponse> GetByIdAsync(string orderId)
        {
            var result = await _amazonClient.GetPayment(orderId);
            return result;
        }

        public async Task<bool> AddAsync(Payment payment)
        {
           var result = await  _amazonClient.PutItem(payment);
            if(result== null) 
                return false;
          ///  _context.Payments.Add(payment);
           // await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Entry(payment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
