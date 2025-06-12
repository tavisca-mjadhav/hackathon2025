using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Mvc;
using PaymentApi.Interfaces;
using PaymentApi.Models;
using System.ComponentModel.DataAnnotations;

namespace PaymentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ICloudWatchLogger _logger;
        private readonly ILogger<PaymentController> _loggerseri;

        public PaymentController(IPaymentService paymentService, ICloudWatchLogger logger, ILogger<PaymentController> loggerseri)
        {
            _paymentService = paymentService;
            _logger = logger;
            _loggerseri = loggerseri;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> Get(string orderId)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(orderId);
            if (payment == null)
                return NotFound();
            return Ok(payment);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            try
            {
                var created = await _paymentService.CreatePaymentAsync(payment);
                return CreatedAtAction(nameof(Create), new { id = payment.Id.ToString() }, created);
            }
            catch (ValidationException ex)
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
        [HttpGet("getIsFault/{isFaultInjection}")]
        public IActionResult Health(bool isFaultInjection)
        {
            if (isFaultInjection)
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
