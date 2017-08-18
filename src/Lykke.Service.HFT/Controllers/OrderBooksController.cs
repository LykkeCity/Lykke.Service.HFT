using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.Assets;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.Controllers
{
	[Route("api/[controller]")]
	public class OrderBooksController : Controller
	{
		private readonly IOrderBooksService _orderBooksService;
		private readonly IAssetPairsManager _assetPairsManager;

		public OrderBooksController(IOrderBooksService apiKeyGenerator, IAssetPairsManager assetPairsManager)
		{
			_orderBooksService = apiKeyGenerator ?? throw new ArgumentNullException(nameof(apiKeyGenerator));
			_assetPairsManager = assetPairsManager ?? throw new ArgumentNullException(nameof(assetPairsManager));
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
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetOrderBook(string assetPairId)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var assetPair = await _assetPairsManager.TryGetEnabledPairAsync(assetPairId);
			if (assetPair == null)
			{
				return NotFound();
			}

			var orderBooks = await _orderBooksService.GetAsync(assetPairId);
			return Ok(orderBooks);
		}
	}
}
