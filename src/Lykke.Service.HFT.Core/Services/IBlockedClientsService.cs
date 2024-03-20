using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IBlockedClientsService
    {
        Task<bool> IsClientBlocked(string clientId);
    }
}
