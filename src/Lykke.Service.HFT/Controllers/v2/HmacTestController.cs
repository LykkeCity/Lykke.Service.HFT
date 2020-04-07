using System.Net;
using Lykke.Service.HFT.Contracts.v2;
using Lykke.Service.HFT.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.HFT.Controllers.v2
{
    [Route("api/v2/hmactest")]
    [ApiVersion("2")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Authorize(AuthenticationSchemes = HmacAuthOptions.AuthenticationScheme)]
    public class HmacTestController : Controller
    {
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public ActionResult Test([FromForm]TestHmacModel hmacModel)
        {
            return Ok();
        }
    }
}
