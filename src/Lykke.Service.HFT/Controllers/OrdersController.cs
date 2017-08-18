using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.Assets;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
		[ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
		public async Task<IActionResult> PlaceLimitOrder([FromBody] LimitOrderRequest order)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ToResponseModel(ModelState));
			}

			var assetPair = await _assetPairsManager.TryGetEnabledPairAsync(order.AssetPairId);
			if (assetPair == null)
			{
				var model = ResponseModel.CreateFail(ResponseModel.ErrorCodeType.UnknownAsset);
				return BadRequest(model);
			}

			var clientId = User.GetUserId();
			var response = await _matchingEngineAdapter.PlaceLimitOrderAsync(
				clientId: clientId,
				assetPairId: order.AssetPairId,
				orderAction: order.OrderAction,
				volume: order.Volume,
				price: order.Price);
			if (response.Error != null)
			{
				return BadRequest(response);
			}

			return Ok(response.Result);
		}

		private static ResponseModel ToResponseModel(ModelStateDictionary modelState)
		{
			var field = modelState.Keys.First();
			var message = modelState[field].Errors.First().ErrorMessage;
			return ResponseModel.CreateInvalidFieldError(field, message);
		}

	}
}
