using System;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.HFT.Controllers
{
    /// <summary>
    /// For debug purpose only.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DebugController : Controller
    {
        private readonly IMatchingEngineAdapter _matchingEngineAdapter;

        public DebugController(
            IMatchingEngineAdapter matchingEngine)
        {
            _matchingEngineAdapter = matchingEngine ?? throw new ArgumentNullException(nameof(matchingEngine));
        }

        [HttpPost("CashIn")]
        public async Task CashIn(string clientId, string assetId, double amount)
        {
            var responseModel = await _matchingEngineAdapter.CashInOutAsync(clientId, assetId, amount);
        }

        [HttpPost("Transfer")]
        public async Task Transfer(string fromClientId, string toClientId, string assetId, double amount)
        {
            var responseModel = await _matchingEngineAdapter.TransferAsync(fromClientId, toClientId, assetId, amount);
        }
    }
}
