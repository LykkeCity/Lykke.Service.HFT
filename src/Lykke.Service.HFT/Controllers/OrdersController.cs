using System;
using System.Threading.Tasks;
using Common;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models;
using Lykke.Service.HFT.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	public class OrdersController : Controller
	{
		private readonly IMatchingEngineAdapter _matchingEngineAdapter;
		private readonly CachedDataDictionary<string, IAssetPair> _assetPairs;

		public OrdersController(IMatchingEngineAdapter frequencyTradingService, CachedDataDictionary<string, IAssetPair> assetPairs)
		{
			_assetPairs = assetPairs ?? throw new ArgumentNullException(nameof(assetPairs));
			_matchingEngineAdapter = frequencyTradingService ?? throw new ArgumentNullException(nameof(frequencyTradingService));
		}

		/// <summary>
		/// Handle market order.
		/// </summary>
		[HttpPost("HandleMarketOrder")]
		[SwaggerOperation("HandleMarketOrder")]
		public async Task HandleMarketOrder([FromBody] HandleMarketOrderRequest request)
		{
			var clientId = User.GetUserId();
			var orderAction = request.Volume > 0
				? OrderAction.Buy
				: OrderAction.Sell;
			var volume = Math.Abs(request.Volume);
			// order.Straight, (double)offchainTransfer.Amount)
			await _matchingEngineAdapter.HandleMarketOrderAsync(
				clientId: clientId,
				assetPairId: request.AssetPair,
				orderAction: orderAction,
				volume: volume,
				straight: request.Straight);
		}

		/// <summary>
		/// Place limit order.
		/// </summary>
		[HttpPost("PlaceLimitOrder")]
		[SwaggerOperation("PlaceLimitOrder")]
		public async Task<IActionResult> PlaceLimitOrder([FromBody] LimitOrderRequest order)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}


			//var assetPair = await _assetPairs.GetItemAsync(order.AssetPairId);
			//if (assetPair == null)
			//{
			//	return BadRequest(ModelState);
			//}
			
			var clientId = User.GetUserId();
			await _matchingEngineAdapter.PlaceLimitOrderAsync(
				clientId: clientId,
				assetPairId: order.AssetPairId,
				orderAction: order.OrderAction,
				volume: order.Volume,
				price: order.Price);
			return Ok();
		}

	}
}
