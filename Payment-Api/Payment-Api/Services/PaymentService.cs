using Amazon.DynamoDBv2.Model;
using PaymentApi.Interfaces;
using PaymentApi.Models;

namespace PaymentApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly ICloudWatchLogger _logger;

        public PaymentService(IPaymentRepository repository, ICloudWatchLogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<GetItemResponse?> GetPaymentByIdAsync(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreatePaymentAsync(Payment payment)
        {
            Validate(payment);
            var created = await _repository.AddAsync(payment);
           await _logger.LogInfoAsync($"Created payment with ID: {payment.Id}");
            return created;
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            Validate(payment);
            await _repository.UpdateAsync(payment);
             await _logger.LogInfoAsync($"Updated payment with ID: {payment.Id}");
        }

        public async Task DeletePaymentAsync(int id)
        {
            await _repository.DeleteAsync(id);
             await _logger.LogInfoAsync("Deleted payment with ID: {id}");
        }

        private void Validate(Payment payment)
        {
            if (payment.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");
            if (string.IsNullOrWhiteSpace(payment.Currency))
                throw new ArgumentException("Currency is required.");
            if (payment.Card == null || string.IsNullOrWhiteSpace(payment.Card.CVV))
                throw new ArgumentException("Card token is required.");
            //if (payment.Card.Expiry == null || payment.Card.Expiry.Year < DateTime.UtcNow.Year)
            //    throw new ArgumentException("Card expiry is invalid.");
        }
    }
}
