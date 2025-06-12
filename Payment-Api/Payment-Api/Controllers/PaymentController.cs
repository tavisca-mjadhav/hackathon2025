using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Mvc;
using PaymentApi.Interfaces;
using PaymentApi.Models;

namespace PaymentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ICloudWatchLogger _logger;

        public PaymentController(IPaymentService paymentService, ICloudWatchLogger logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();
            return Ok(payment);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            try
            {

                await _logger.LogInfoAsync("Payment Api create start", new Dictionary<string, object> { { "CorrelationId", Guid.NewGuid() } });
                var created = await _paymentService.CreatePaymentAsync(payment);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                await _logger.LogErrorAsync("Validation failed.",ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Payment payment)
        {
            if (id != payment.Id)
                return BadRequest("ID mismatch");

            try
            {
                await _paymentService.UpdatePaymentAsync(payment);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
               await _logger.LogErrorAsync("Validation failed.",ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _paymentService.DeletePaymentAsync(id);
            return NoContent();
        }
    }
}
