using System;
using System.Threading.Tasks;
using Lykke.Service.HFT.Abstractions.Services;
using Lykke.Service.HFT.WebApi.Helpers;
using Lykke.Service.HFT.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.WebApi.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	public class HighFrequencyTradingController : Controller
	{
		private readonly IMatchingEngineAdapter _frequencyTradingService;

		public HighFrequencyTradingController(IMatchingEngineAdapter frequencyTradingService)
		{
			_frequencyTradingService = frequencyTradingService ?? throw new ArgumentNullException(nameof(frequencyTradingService));
		}

		/// <summary>
		/// Checks ME is connected
		/// </summary>
		/// <returns>true, if connected.</returns>
		[HttpGet("IsConnected")]
		[SwaggerOperation("IsConnected")]
		public bool IsConnected()
		{
			return _frequencyTradingService.IsConnected;
		}

		/// <summary>
		/// Place limit order.
		/// </summary>
		[HttpPost("PlaceLimitOrder")]
		[SwaggerOperation("PlaceLimitOrder")]
		public async Task PlaceLimitOrder([FromBody] LimitOrderRequest order)
		{
			var clientId = User.GetUserId();
			await _frequencyTradingService.PlaceLimitOrderAsync(
				clientId: clientId,
				assetPairId: order.AssetPairId,
				orderAction: order.OrderAction,
				volume: order.Volume,
				price: order.Price);
		}
	}
}
