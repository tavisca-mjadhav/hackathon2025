using Incident_Analyzer_Bff.Handler;
using Microsoft.AspNetCore.Mvc;

namespace Incident_Analyzer_Bff.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnayzerController : ControllerBase
    {
        private readonly ILogger<AnayzerController> _logger;

        public AnayzerController(ILogger<AnayzerController> logger)
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
