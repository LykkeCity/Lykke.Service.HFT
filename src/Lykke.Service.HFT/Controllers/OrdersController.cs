using System;
using System.Threading.Tasks;
using Common;
using Lykke.Service.Assets.Client.Custom;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
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
		private readonly IAssetPairsManager _assetPairsManager;

		public OrdersController(IMatchingEngineAdapter frequencyTradingService, IAssetPairsManager assetPairsManager)
		{
			_matchingEngineAdapter = frequencyTradingService ?? throw new ArgumentNullException(nameof(frequencyTradingService));
			_assetPairsManager = assetPairsManager ?? throw new ArgumentNullException(nameof(assetPairsManager));
		}

		/// <summary>
		/// Place limit order.
		/// </summary>
		/// <returns>Request id.</returns>
		[HttpPost("PlaceLimitOrder")]
		[SwaggerOperation("PlaceLimitOrder")]
		[Produces(typeof(string))]
		public async Task<IActionResult> PlaceLimitOrder([FromBody] LimitOrderRequest order)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var assetPair = await _assetPairsManager.TryGetEnabledPairAsync(order.AssetPairId);
			if (assetPair == null)
			{
				return BadRequest(new ResponseModel { Status = StatusCodes.UnknownAsset, Message = $"Unknown asset pair '{order.AssetPairId}'" });
			}

			var clientId = User.GetUserId();
			var response = await _matchingEngineAdapter.PlaceLimitOrderAsync(
				clientId: clientId,
				assetPairId: order.AssetPairId,
				orderAction: order.OrderAction,
				volume: order.Volume,
				price: order.Price);
			if (response.Status != StatusCodes.Ok)
			{
				return BadRequest(response);
			}

			return Ok(response.Result);
		}

	}
}
