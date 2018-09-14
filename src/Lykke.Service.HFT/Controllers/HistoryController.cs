using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Contracts.History;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.OperationsHistory.AutorestClient.Models;
using Lykke.Service.OperationsHistory.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
        private const int MaxPageSize = 1000;
        private const int MaxSkipSize = MaxPageSize * 1000;
        private readonly IOperationsHistoryClient _operationsHistoryClient;
        private readonly IAssetsReadModel _assetsReadModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryController"/> class.
        /// </summary>
        public HistoryController(
            IOperationsHistoryClient operationsHistoryClient,
            IAssetsReadModel assetsReadModel)
        {
            _operationsHistoryClient = operationsHistoryClient;
            _assetsReadModel = assetsReadModel;
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

            if (assetId != null && _assetsReadModel.GetIfEnabled(assetId) == null)
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
        /// <response code="200">The requested historic client trades.</response>
        /// <response code="404">Specified asset or asset pair could not be found.</response>
        [HttpGet("trades/{tradeId}")]
        [SwaggerOperation(nameof(GetTrade))]
        [ProducesResponseType(typeof(HistoryTradeModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
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
