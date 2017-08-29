using System;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services
{
	public class ApiKeyService : IApiKeyValidator, IApiKeyGenerator, IClientResolver
	{
		private readonly IDistributedCache _distributedCache;
		private readonly AppSettings.HighFrequencyTradingSettings _settings;
	    private readonly IRepository<ApiKey> _apiKeyRepository;

        public ApiKeyService(IDistributedCache distributedCache, AppSettings.HighFrequencyTradingSettings settings, IRepository<ApiKey> orderStateRepository)
		{
		    _settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
		    _apiKeyRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
        }

		public async Task<bool> ValidateAsync(string apiKey)
		{
			var clientId = await GetClientAsync(apiKey);
			return clientId != null;
		}

		public async Task<string> GenerateApiKeyAsync(string clientId)
		{
		    var apiKey = Guid.NewGuid();
            var apiKeyAsString = apiKey.ToString();
            await _distributedCache.SetStringAsync(GetCacheKey(apiKeyAsString), clientId);
		    var existedApiKey = await _apiKeyRepository.Get(x => x.ClientId == clientId && x.ValidTill == null );
		    if (existedApiKey != null)
		    {
		        await _distributedCache.RemoveAsync(GetCacheKey(existedApiKey.Id.ToString()));
		        existedApiKey.ValidTill = DateTime.UtcNow;
		        await _apiKeyRepository.Update(existedApiKey);
		    }
		    await _apiKeyRepository.Add(new ApiKey {Id = apiKey, ClientId = clientId});

            return apiKeyAsString;
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
			if (clientId == null && apiKey == _settings.ApiKey)
			{
				return _settings.ApiKey;
			}
			return clientId;
		}
	}
}
