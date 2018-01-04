using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models;
using Lykke.Service.OperationsHistory.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class HistoryController : Controller
    {
        private readonly IOperationsHistoryClient _operationsHistoryClient;
        private readonly IAssetsServiceWithCache _assetsServiceClient;

        public HistoryController(
            IOperationsHistoryClient operationsHistoryClient,
            IAssetsServiceWithCache assetsServiceClient)
        {
            _operationsHistoryClient = operationsHistoryClient;
            _assetsServiceClient = assetsServiceClient;
        }

        /// <summary>
        /// Get trades
        /// </summary>
        /// <param name="assetId">Asset identifier</param>
        /// <param name="take">How many maximum items have to be returned</param>
        /// <param name="skip">How many items skip before returning</param>
        /// <returns></returns>
        [HttpGet("trades")]
        [SwaggerOperation("GetTrades")]
        [ProducesResponseType(typeof(IEnumerable<HistoryTradeModel>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTrades([FromQuery] string assetId, [FromQuery] int take,
            [FromQuery] int skip)
        {
            var asset = await _assetsServiceClient.TryGetAssetAsync(assetId);
            if (asset == null)
            {
                return NotFound();
            }

            var walletId = User.GetUserId();

            var response = await _operationsHistoryClient.GetByWalletId(walletId, nameof(OperationType.ClientTrade), assetId, take, skip);

            if (response.Error != null)
            {
                return BadRequest(ErrorResponse.Create(response.Error.Message));
            }

            return Ok(response.Records.Select(x => x.ConvertToApiModel()).Where(x => x != null));
        }

        /// <summary>
        /// Get trade details by id
        /// </summary>
        /// <param name="tradeId">Trade identifier</param>
        /// <returns></returns>
        [HttpGet("trades/{tradeId}")]
        [SwaggerOperation("GetTrade")]
        [ProducesResponseType(typeof(HistoryTradeModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTrade(string tradeId)
        {
            var walletId = User.GetUserId();

            var response = await _operationsHistoryClient.GetByOperationId(walletId, tradeId);

            if (response == null || response.Trade == null || response.OpType != nameof(OperationType.ClientTrade))
            {
                return NotFound();
            }

            return Ok(response.ConvertToApiModel());
        }
    }
}
