using FraudCheckAPI.Log;
using FraudCheckAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FraudCheckAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FraudController : ControllerBase
    {
        private readonly IFraudCheckService _fraudCheckService;
        private readonly CloudWatchLogger _cloudWatchLogs;

        public FraudController(IFraudCheckService fraudCheckService, CloudWatchLogger cloudWatchLogs)
        {
            _fraudCheckService = fraudCheckService;
            _cloudWatchLogs = cloudWatchLogs;
        }

        [HttpPost("check")]
        public async Task<ActionResult<FraudCheckResponse>> CheckFraud([FromBody] FraudCheckRequest request)
        {
            var result = await _fraudCheckService.Check(request);

            _cloudWatchLogs.LogInfoAsync("Fraud check result: {Result}", result);

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
