using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Contracts.OrderBook;
using Lykke.Service.HFT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Controllers
{
    /// <summary>
    /// Controller for orderbooks functionality.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderBooksController : Controller
    {
        private readonly IOrderBooksService _orderBooksService;
        private readonly IAssetServiceDecorator _assetServiceDecorator;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderBooksController"/> class.
        /// </summary>
        public OrderBooksController(IOrderBooksService orderBooksService, IAssetServiceDecorator assetServiceDecorator)
        {
            _orderBooksService = orderBooksService ?? throw new ArgumentNullException(nameof(orderBooksService));
            _assetServiceDecorator = assetServiceDecorator ?? throw new ArgumentNullException(nameof(assetServiceDecorator));
        }

        /// <summary>
        /// Get all order books.
        /// </summary>
        /// <response code="200">All orderbooks.</response>
        [HttpGet]
        [SwaggerOperation(nameof(GetOrderBooks))]
        [ProducesResponseType(typeof(IEnumerable<OrderBookModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetOrderBooks()
        {
            var orderBooks = await _orderBooksService.GetAllAsync();
            return Ok(orderBooks);
        }

        /// <summary>
        /// Get order books for a specified asset pair.
        /// </summary>
        /// <param name="assetPairId">Asset pair ID. Example: AUDUSD</param>
        /// <returns>Order books for a specified asset pair.</returns>
        /// <response code="200">Order books for a specified asset pair.</response>
        /// <response code="404">Unknow or disabled asset pair.</response>
        [HttpGet("{assetPairId}")]
        [SwaggerOperation(nameof(GetOrderBook))]
        [ProducesResponseType(typeof(IEnumerable<OrderBookModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetOrderBook(string assetPairId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assetPair = await _assetServiceDecorator.GetEnabledAssetPairAsync(assetPairId);
            if (assetPair == null)
            {
                return NotFound();
            }

            var orderBooks = await _orderBooksService.GetAsync(assetPairId);
            return Ok(orderBooks);
        }
    }
}
