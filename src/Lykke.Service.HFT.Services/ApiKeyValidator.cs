using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;

namespace Lykke.Service.HFT.Services
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly IHftClientService _hftClientService;

        public ApiKeyValidator(IHftClientService hftClientService)
        {
            _hftClientService = hftClientService;
        }

        public async Task<bool> ValidateAsync(string apiKey)
        {
            var walletId = await _hftClientService.GetWalletIdAsync(apiKey);
            return walletId != null;
        }
    }
}
