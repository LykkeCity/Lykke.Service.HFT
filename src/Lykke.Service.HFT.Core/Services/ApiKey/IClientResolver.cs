using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services.ApiKey
{
    public interface IClientResolver
    {
        Task<string> GetWalletIdAsync(string apiKey);
        Task<bool> IsHftWalletAsync(string walletId);
    }
}
