using System;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.HFT.Controllers
{
	[Route("api/[controller]")]
	public class ApiKeysController : Controller
	{
		private readonly IApiKeyGenerator _apiKeyGenerator;

		public ApiKeysController(IApiKeyGenerator apiKeyGenerator)
		{
			_apiKeyGenerator = apiKeyGenerator ?? throw new ArgumentNullException(nameof(apiKeyGenerator));
		}

		/// <summary>
		/// Generate an API key for specified client.
		/// </summary>
		/// <returns>API key.</returns>
		[HttpPost("GenerateKey/{clientId}")]
		public async Task<string> GenerateKey(string clientId)
		{
			return await _apiKeyGenerator.GenerateApiKeyAsync(clientId);
		}
	}
}
