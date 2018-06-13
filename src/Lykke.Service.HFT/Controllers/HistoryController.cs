using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models;
using Lykke.Service.OperationsHistory.AutorestClient.Models;
using Lykke.Service.OperationsHistory.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class HistoryController : Controller
    {
        private const int MaxPageSize = 1000;
        private const int MaxSkipSize = MaxPageSize * 1000;
        private readonly IOperationsHistoryClient _operationsHistoryClient;
        private readonly IAssetServiceDecorator _assetServiceDecorator;

        public HistoryController(
            IOperationsHistoryClient operationsHistoryClient,
            IAssetServiceDecorator assetServiceDecorator)
        {
            _operationsHistoryClient = operationsHistoryClient;
            _assetServiceDecorator = assetServiceDecorator;
        }

        /// <summary>
        /// Get trades
        /// </summary>
        /// <param name="assetId">Asset identifier</param>
        /// <param name="assetPairId">Asset-pair identifier</param>
        /// <param name="take">How many maximum items have to be returned</param>
        /// <param name="skip">How many items skip before returning</param>
        /// <returns></returns>
        [HttpGet("trades")]
        [SwaggerOperation("GetTrades")]
        [ProducesResponseType(typeof(IEnumerable<HistoryTradeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTrades(
            [FromQuery] string assetId, 
            [FromQuery] string assetPairId = null, 
            [FromQuery] int? skip = 0, 
            [FromQuery] int? take = 100)
        {
            var toTake = take.ValidateAndGetValue(nameof(take), MaxPageSize, 100);
            if (toTake.Error != null)
            {
                return BadRequest(new ResponseModel { Error = toTake.Error });
            }

            var toSkip = skip.ValidateAndGetValue(nameof(skip), MaxSkipSize, 0);
            if (toSkip.Error != null)
            {
                return BadRequest(new ResponseModel { Error = toSkip.Error });
            }

            if (assetId != null && await _assetServiceDecorator.GetAssetAsync(assetId) == null)
            {
                return NotFound();
            }

            var walletId = User.GetUserId();

            var response = await _operationsHistoryClient.GetByWalletId(
                walletId: walletId,
                operationType: null,
                assetId: assetId,
                assetPairId: assetPairId,
                take: toTake.Result,
                skip: toSkip.Result);

            if (response.Error != null)
            {
                return BadRequest(ResponseModel.CreateFail(ResponseModel.ErrorCodeType.Runtime, response.Error.Message));
            }

            return Ok(response.Records.Where(x => x.Type == HistoryOperationType.Trade || x.Type == HistoryOperationType.LimitTrade)
                .Select(x => x.ConvertToApiModel()));
        }

        /// <summary>
        /// Get trade details by id
        /// </summary>
        /// <param name="tradeId">Trade identifier</param>
        /// <returns></returns>
        [HttpGet("trades/{tradeId}")]
        [SwaggerOperation("GetTrade")]
        [ProducesResponseType(typeof(HistoryTradeModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTrade(string tradeId)
        {
            if (tradeId == null)
            {
                return NotFound();
            }

            var walletId = User.GetUserId();

            var response = await _operationsHistoryClient.GetByOperationId(walletId, tradeId);
            if (response == null)
            {
                return NotFound();
            }

            return Ok(response.ConvertToApiModel());
        }
    }
}
