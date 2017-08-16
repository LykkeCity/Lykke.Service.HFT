using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.Controllers
{
	[Route("api/[controller]")]
	public class OrderBooksController : Controller
	{
		private readonly IOrderBooksService _orderBooksService;
		private readonly CachedDataDictionary<string, IAssetPair> _assetPairs;

		public OrderBooksController(IOrderBooksService apiKeyGenerator, CachedDataDictionary<string, IAssetPair> assetPairs)
		{
			_assetPairs = assetPairs ?? throw new ArgumentNullException(nameof(assetPairs));
			_orderBooksService = apiKeyGenerator ?? throw new ArgumentNullException(nameof(apiKeyGenerator));
		}

		/// <summary>
		/// Get all order books.
		/// </summary>
		/// <returns>All order books.</returns>
		[HttpGet]
		[SwaggerOperation("OrderBooks")]
		[Produces(typeof(IEnumerable<IOrderBook>))]
		public async Task<IActionResult> GetOrderBooks()
		{
			var orderBooks = await _orderBooksService.GetAllAsync();
			return Ok(orderBooks);

		}

		/// <summary>
		/// Get order books for a specified asster pair.
		/// </summary>
		/// <returns>Order books for a specified asster pair.</returns>
		[HttpGet("{assetPairId}")]
		[SwaggerOperation("OrderBooks_id")]
		[Produces(typeof(IEnumerable<IOrderBook>))]
		public async Task<IActionResult> GetOrderBooks(string assetPairId)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			
			var assetPair = await _assetPairs.GetItemAsync(assetPairId);
			if (assetPair == null)
			{
				return NotFound(assetPairId);
			}
			
			var orderBooks = await _orderBooksService.GetAsync(assetPairId);
			return Ok(orderBooks);
		}
	}
}
