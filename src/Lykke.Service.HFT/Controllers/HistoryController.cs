using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Contracts.History;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.History.Client;
using Lykke.Service.History.Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using FeeType = Lykke.Service.HFT.Contracts.History.FeeType;

namespace Lykke.Service.HFT.Controllers
{
    /// <summary>
    /// Controller for accessing historic trades.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : Controller
    {
        private const int MaxPageSize = 500;
        private const int MaxSkipSize = 500;
        private readonly IHistoryClient _historyClient;
        private readonly IAssetsReadModelRepository _assetsReadModel;
        private readonly IAssetPairsReadModelRepository _assetPairsReadModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryController"/> class.
        /// </summary>
        public HistoryController(
            IHistoryClient historyClient,
            IAssetsReadModelRepository assetsReadModel,
            IAssetPairsReadModelRepository assetPairsReadModel)
        {
            _assetsReadModel = assetsReadModel;
            _assetPairsReadModel = assetPairsReadModel;
            _historyClient = historyClient;
        }

        /// <summary>
        /// Query historic client trades.
        /// </summary>
        /// <param name="assetId">The asset identifier, eg BTC</param>
        /// <param name="assetPairId">The asset pair identifier, eg EOSBTC</param>
        /// <param name="take">How many maximum items have to be returned, max 1000 default 100.</param>
        /// <param name="skip">How many items skip before returning, default 0.</param>
        /// <response code="200">The requested historic client trades.</response>
        /// <response code="404">Specified asset or asset pair could not be found.</response>
        [HttpGet("trades")]
        [SwaggerOperation(nameof(GetTrades))]
        [ProducesResponseType(typeof(IEnumerable<HistoryTradeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTrades(string assetId, string assetPairId = null, int? skip = 0, int? take = 100)
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

            if (assetId != null && _assetsReadModel.TryGetIfEnabled(assetId) == null)
            {
                return NotFound();
            }

            if (assetPairId != null && _assetPairsReadModel.TryGetIfEnabled(assetPairId) == null)
            {
                return NotFound();
            }

            var walletId = Guid.Parse(User.GetUserId());

            var response = await _historyClient.HistoryApi.GetHistoryByWalletAsync(
                walletId: walletId,
                new[] { HistoryType.Trade },
                assetId: assetId,
                assetPairId: assetPairId,
                offset: toSkip.Result,
                limit: toTake.Result);

            var result = response
                .Cast<History.Contracts.History.TradeModel>()
                .Select(ToModel);

            return Ok(result);
        }

        private static HistoryTradeModel ToModel(History.Contracts.History.TradeModel src)
        {
            return new HistoryTradeModel
            {
                Id = src.Id,
                OrderId = src.OrderId,
                DateTime = src.Timestamp,
                Timestamp = src.Timestamp,
                Amount = src.BaseVolume,
                BaseVolume = src.BaseVolume,
                QuotingVolume = src.QuotingVolume,
                Asset = src.BaseAssetId,
                BaseAssetId = src.BaseAssetId,
                QuotingAssetId = src.QuotingAssetId,
                AssetPair = src.AssetPairId,
                AssetPairId = src.AssetPairId,
                Price = src.Price,
                Fee = new FeeModel
                {
                    Amount = src.FeeSize,
                    Type = src.FeeSize.HasValue ? FeeType.Absolute : FeeType.Unknown,
                    FeeAssetId = src.FeeAssetId
                }
            };
        }

    }
}
