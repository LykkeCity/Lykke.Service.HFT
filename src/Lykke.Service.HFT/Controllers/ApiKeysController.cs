using System;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Lykke.Service.HFT.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.HFT.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	public class ApiKeysController : Controller
	{
		private readonly IApiKeyGenerator _apiKeyGenerator;
		private readonly AppSettings.HighFrequencyTradingSettings _settings;

		public ApiKeysController(IApiKeyGenerator apiKeyGenerator, AppSettings.HighFrequencyTradingSettings settings)
		{
			_apiKeyGenerator = apiKeyGenerator ?? throw new ArgumentNullException(nameof(apiKeyGenerator));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		/// <summary>
		/// Generate an API key for specified client.
		/// </summary>
		/// <param name="clientId">Client ID</param>
		/// <returns>API key for client.</returns>
		/// <remarks>Please use service-defined access token as 'api-key'.</remarks>
		[HttpPost("GenerateKey/{clientId}")]
		[ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.Forbidden)]
		[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
		public async Task<IActionResult> GenerateKey(string clientId)
		{
			var userId = User.GetUserId();
			if(userId != _settings.ApiKey)
			{
				return Forbid();
			}

			var apiKey = await _apiKeyGenerator.GenerateApiKeyAsync(clientId);
			return Ok(apiKey);
		}
	}
}
