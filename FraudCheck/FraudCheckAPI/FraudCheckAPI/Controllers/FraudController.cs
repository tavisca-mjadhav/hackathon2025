using FraudCheckAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FraudCheckAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FraudController : ControllerBase
    {
        private readonly IFraudCheckService _fraudCheckService;

        public FraudController(IFraudCheckService fraudCheckService)
        {
            _fraudCheckService = fraudCheckService;
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> CheckFraud(string orderId)
        {
            var result = await _fraudCheckService.CheckFraudAsync(orderId);
            return Ok(result);
        }

        [HttpGet("getIsFault/{isFaultInjection}")]
        public IActionResult HealthCheck(bool isFaultInjection)
        {
            if (isFaultInjection)
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
