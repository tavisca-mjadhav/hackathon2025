using Incident_Analyzer_Bff.Handler;
using Microsoft.AspNetCore.Mvc;

namespace Incident_Analyzer_Bff.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnalyzerController : ControllerBase
    {
        private readonly ILogger<AnalyzerController> _logger;

        public AnalyzerController(ILogger<AnalyzerController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetDetails")]
        public string Get(string cid)
        {
            AnalyzerHandler analyzerHandler = new AnalyzerHandler();
            return analyzerHandler.GetErrorDetails(cid).Result;
        }
    }
}
