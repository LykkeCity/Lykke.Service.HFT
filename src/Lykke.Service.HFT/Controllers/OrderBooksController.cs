﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.Assets;
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
        [SwaggerOperation("OrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<OrderBook>), (int)HttpStatusCode.OK)]
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
        [HttpGet("{assetPairId}")]
        [SwaggerOperation("OrderBooks_id")]
        [ProducesResponseType(typeof(IEnumerable<OrderBook>), (int)HttpStatusCode.OK)]
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
