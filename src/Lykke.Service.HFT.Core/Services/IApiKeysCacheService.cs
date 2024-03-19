using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IApiKeysCacheService
    {
        Task<string> GetWalletIdAsync(string apiKey);
        Task AddKey(string apiKey, string walletId);
        Task RemoveKey(string apiKey);
        Task AddApiKeys(IReadOnlyList<Domain.ApiKey> keys);
        Task Clear();
    }
}
