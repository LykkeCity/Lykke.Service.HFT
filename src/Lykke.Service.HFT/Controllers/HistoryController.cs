using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Contracts.History;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.OperationsHistory.AutorestClient.Models;
using Lykke.Service.OperationsHistory.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using FeeType = Lykke.Service.HFT.Contracts.History.FeeType;

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
                return BadRequest(ResponseModel.CreateFail(ErrorCodeType.Runtime, response.Error.Message));
            }

            var result = response.Records
                .Where(x => x.Type == HistoryOperationType.Trade || x.Type == HistoryOperationType.LimitTrade)
                .Select(ToModel);

            return Ok(result);
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

            return Ok(ToModel(response));
        }

        private static HistoryTradeModel ToModel(HistoryOperation src)
        {
            return new HistoryTradeModel
            {
                Id = src.Id,
                DateTime = src.DateTime,
                State = src.State.ConvertToEnum(TradeStatus.Unknown),
                Amount = src.Amount,
                Asset = src.Asset,
                AssetPair = src.AssetPair,
                Price = src.Price,
                Fee = new FeeModel
                {
                    Amount = src.FeeSize,
                    Type = src.FeeType.ConvertToEnum(FeeType.Unknown)
                }
            };
        }

    }
}
