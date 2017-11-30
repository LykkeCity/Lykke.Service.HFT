using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services.ApiKey
{
    public interface IClientResolver
    {
        Task<string> GetClientAsync(string apiKey);
        Task<string> GetNotificationIdAsync(string walletId);
        Task SetNotificationIdAsync(string apiKey, string notificationId);
    }
}
