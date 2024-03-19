using System.Threading.Tasks;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.HFT.Core.Services;

namespace Lykke.Service.HFT.Services
{
    public class BlockedClientsService : IBlockedClientsService
    {
        private readonly IClientAccountClient _clientAccountClient;

        public BlockedClientsService(IClientAccountClient clientAccountClient)
        {
            _clientAccountClient = clientAccountClient;
        }

        public async Task<bool> IsClientBlocked(string clientId)
        {
            return (await _clientAccountClient.ClientSettings.GetCashOutBlockSettingsAsync(clientId))?.TradesBlocked == true;
        }
    }
}
