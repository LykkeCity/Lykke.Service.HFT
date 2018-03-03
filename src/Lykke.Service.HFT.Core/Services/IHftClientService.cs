using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IHftClientService
    {
        Task<string> GetWalletIdAsync(string apiKey);
        Task<bool> IsHftWalletAsync(string walletId);
    }
}
