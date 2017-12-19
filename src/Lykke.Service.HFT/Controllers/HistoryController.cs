using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models;
using Lykke.Service.OperationsHistory.Client;
using Lykke.Service.OperationsRepository.Contract;
using Lykke.Service.OperationsRepository.Contract.Cash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class HistoryController : Controller
    {
        private readonly IOperationsHistoryClient _operationsHistoryClient;

        public HistoryController(IOperationsHistoryClient operationsHistoryClient)
        {
            _operationsHistoryClient = operationsHistoryClient;
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
        public async Task<IActionResult> GetTrades([FromQuery] string assetId, [FromQuery] int take, [FromQuery] int skip)
        {
            var walletId = User.GetUserId();

            var response = await _operationsHistoryClient.GetByWalletId(walletId, nameof(OperationType.ClientTrade),
                assetId, take, skip);

            if (response.Error != null)
            {
                return BadRequest(ErrorResponse.Create(response.Error.Message));
            }

            return Ok(response.Records.Select(x =>
                NetJSON.NetJSON.Deserialize<ClientTradeDto>(x.CustomData).ConvertToApiModel()));
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

            if (response == null || response.OpType != nameof(OperationType.ClientTrade))
            {
                return NotFound();
            }

            return Ok(NetJSON.NetJSON.Deserialize<ClientTradeDto>(response.CustomData).ConvertToApiModel());
        }
    }
}
