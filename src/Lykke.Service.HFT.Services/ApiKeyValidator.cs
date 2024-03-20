using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;

namespace Lykke.Service.HFT.Services
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly IApiKeysCacheService _apiKeysCacheService;

        public ApiKeyValidator(IApiKeysCacheService apiKeysCacheService)
        {
            _apiKeysCacheService = apiKeysCacheService;
        }

        public async Task<bool> ValidateAsync(string apiKey)
        {
            var walletId = await _apiKeysCacheService.GetWalletIdAsync(apiKey);
            return walletId != null;
        }
    }
}
