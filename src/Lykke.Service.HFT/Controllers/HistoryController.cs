using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class HistoryController : Controller
    {
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
        public Task<IActionResult> GetTrades([FromQuery] string assetId, [FromQuery] int take, [FromQuery] int skip)
        {
            throw  new NotImplementedException();
        }

        /// <summary>
        /// Get trade details by id
        /// </summary>
        /// <param name="tradeId">Trade identifier</param>
        /// <returns></returns>
        [HttpGet("trades/{tradeId}")]
        [SwaggerOperation("GetTrade")]
        [ProducesResponseType(typeof(HistoryTradeModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public Task<IActionResult> GetTrade(string tradeId)
        {
            throw  new NotImplementedException();
        }
    }
}
