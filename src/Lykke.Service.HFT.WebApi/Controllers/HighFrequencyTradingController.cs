using System;
using Lykke.Service.HFT.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.WebApi.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	public class HighFrequencyTradingController : Controller
	{
		private readonly IHighFrequencyTradingService _frequencyTradingService;

		public HighFrequencyTradingController(IHighFrequencyTradingService frequencyTradingService)
		{
			_frequencyTradingService = frequencyTradingService ?? throw new ArgumentNullException(nameof(frequencyTradingService));
		}

		/// <summary>
		/// Checks ME is connected
		/// </summary>
		/// <returns></returns>
		[HttpGet("IsConnected")]
		[SwaggerOperation("IsConnected")]
		public bool IsConnected()
		{
			return _frequencyTradingService.IsConnected;
		}
	}
}
