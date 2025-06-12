using Amazon.DynamoDBv2.Model;
using PaymentApi.Interfaces;
using PaymentApi.Models;
using System.ComponentModel.DataAnnotations;

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
                throw new ValidationException("Amount must be greater than zero.");
            if (string.IsNullOrWhiteSpace(payment.Currency))
                throw new ValidationException("Currency is required.");
            if (payment.Card == null || string.IsNullOrWhiteSpace(payment.Card.CVV))
                throw new ValidationException("Card CVV is required.");
            if (payment.Card.Expiry == null || Convert.ToInt32(payment.Card.Expiry.Year) < DateTime.UtcNow.Year)
                throw new ValidationException("Card expiry is invalid.");
            if (payment.Card.Number != "5555 5555 5555 4444")
                throw new ValidationException($"Invalid credit card number :{payment.Card.Number}");
        }
    }
}
