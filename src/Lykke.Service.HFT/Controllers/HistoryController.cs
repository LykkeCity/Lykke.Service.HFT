using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models;
using Lykke.Service.OperationsHistory.AutorestClient.Models;
using Lykke.Service.OperationsHistory.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class HistoryController : Controller
    {
        private const int MaxPageSize = 1000;
        private const int MaxDeepSize = MaxPageSize * 1000;
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
        [ProducesResponseType(typeof(IEnumerable<HistoryTradeModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTrades([FromQuery] string assetId, [FromQuery] uint? skip = 0, [FromQuery] uint? take = 100)
        {
            if (take > MaxPageSize)
            {
                return BadRequest(ResponseModel.CreateInvalidFieldError("take", $"Page size {take} is to big"));
            }

            if (skip > MaxDeepSize)
            {
                return BadRequest(ResponseModel.CreateInvalidFieldError("skip", $"Skip size {take} is to big"));
            }

            if (assetId != null && await _assetsServiceClient.TryGetAssetAsync(assetId) == null)
            {
                return NotFound();
            }

            var walletId = User.GetUserId();

            var response = await _operationsHistoryClient.GetByWalletId(walletId, HistoryOperationType.Trade, assetId, (int)take, (int)skip);

            if (response.Error != null)
            {
                return BadRequest(ResponseModel.CreateFail(ResponseModel.ErrorCodeType.Runtime, response.Error.Message));
            }

            return Ok(response.Records.Where(x => x.Type == HistoryOperationType.Trade).Select(x => x.ConvertToApiModel()));
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
        public async Task<IActionResult> GetTrade([NotNull] string tradeId)
        {
            if (tradeId == null)
            {
                return NotFound();
            }

            var walletId = User.GetUserId();

            var response = await _operationsHistoryClient.GetByOperationId(walletId, tradeId);

            if (response == null || response.Type != HistoryOperationType.Trade)
            {
                return NotFound();
            }

            return Ok(response.ConvertToApiModel());
        }
    }
}
