using System.Net;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Contracts.Health;
using Lykke.Service.HFT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Controllers
{
    /// <summary>
    /// Operations to check if HFT service is alive.
    /// </summary>
    [Route("api/[controller]")]
    public class IsAliveController : Controller
    {
        private readonly IHealthService _healthService;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsAliveController"/> class.
        /// </summary>
        public IsAliveController(IHealthService healthService)
        {
            _healthService = healthService;
        }

        /// <summary>
        /// Checks if the high frequency trading service is alive.
        /// </summary>
        /// <response code="200">The service is alive.</response>
        /// <response code="500">The service is unhealthy.</response>
        [HttpGet]
        [SwaggerOperation("IsAlive")]
        [ProducesResponseType(typeof(IsAliveModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.InternalServerError)]
        public IActionResult Get()
        {
            var healthViloationMessage = _healthService.GetHealthViolationMessage();
            if (healthViloationMessage != null)
            {
                return StatusCode(
                    (int)HttpStatusCode.InternalServerError,
                    new ErrorModel { Message = $"Service is unhealthy: {healthViloationMessage}" });
            }

            // NOTE: Feel free to extend IsAliveResponse, to display job-specific indicators
            return Ok(new IsAliveModel
            {
                Name = Common.AppEnvironment.Name,
                Version = Common.AppEnvironment.Version,
                Env = Common.AppEnvironment.EnvInfo,
#if DEBUG
                IsDebug = true,
#else
                IsDebug = false,
#endif
                IssueIndicators = _healthService.GetHealthIssues()
            });
        }
    }
}
