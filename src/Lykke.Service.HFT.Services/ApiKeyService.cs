using System;
using System.Threading.Tasks;
using Common;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services
{
	public class ApiKeyService : IApiKeyValidator, IApiKeyGenerator, IClientResolver
	{
		private readonly IDistributedCache _distributedCache;
		private readonly HighFrequencyTradingSettings _settings;

		public ApiKeyService(IDistributedCache distributedCache, HighFrequencyTradingSettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
		}

		public async Task<bool> ValidateAsync(string apiKey)
		{
			var clientId = await GetClientAsync(apiKey);
			return clientId != null;
		}

		public async Task<string> GenerateApiKeyAsync(string clientId)
		{
			var apiKey = Guid.NewGuid().ToString();
			await _distributedCache.SetStringAsync(GetCacheKey(apiKey), clientId);
			return apiKey;
		}

		private string GetCacheKey(string apiKey)
		{
			return _settings.CacheSettings.GetApiKey(apiKey);
		}

		public async Task<string> GetClientAsync(string apiKey)
		{
			//var apiKeyJson = _distributedCache.GetString(GetCacheKey(apiKey));
			//var clientId = apiKeyJson?.DeserializeJson<string>();
			var clientId = await _distributedCache.GetStringAsync(GetCacheKey(apiKey));
			return clientId;
		}
	}
}
