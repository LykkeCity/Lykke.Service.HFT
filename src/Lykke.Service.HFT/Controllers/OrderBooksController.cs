using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Contracts.OrderBook;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Controllers
{
    [Route("api/[controller]")]
    public class OrderBooksController : Controller
    {
        private readonly IOrderBooksService _orderBooksService;
        private readonly IAssetServiceDecorator _assetServiceDecorator;

        public OrderBooksController(IOrderBooksService orderBooksService, IAssetServiceDecorator assetServiceDecorator)
        {
            _orderBooksService = orderBooksService ?? throw new ArgumentNullException(nameof(orderBooksService));
            _assetServiceDecorator = assetServiceDecorator ?? throw new ArgumentNullException(nameof(assetServiceDecorator));
        }

        /// <summary>
        /// Get all order books.
        /// </summary>
        /// <returns>All order books.</returns>
        [HttpGet]
        [SwaggerOperation("GetOrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<OrderBookModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetOrderBooks()
        {
            var orderBooks = await _orderBooksService.GetAllAsync();
            return Ok(orderBooks.Select(ToModel));

        }

        /// <summary>
        /// Get order books for a specified asset pair.
        /// </summary>
        /// <param name="assetPairId">Asset pair ID. Example: AUDUSD</param>
        /// <returns>Order books for a specified asset pair.</returns>
        [HttpGet("{assetPairId}")]
        [SwaggerOperation("GetOrderBook")]
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
            return Ok(orderBooks.Select(ToModel));
        }

        private static OrderBookModel ToModel(OrderBook orderBook)
        {
            return new OrderBookModel
            {
                AssetPair = orderBook.AssetPair,
                IsBuy = orderBook.IsBuy,
                Timestamp = orderBook.Timestamp,
                Prices = orderBook.Prices
                    ?.Select(vp => new VolumePriceModel
                    {
                        Price = vp.Price,
                        Volume = vp.Volume
                    })
                    .ToArray()
            };
        }
    }
}
