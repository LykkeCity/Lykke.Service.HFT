using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.HFT.Core.Services;

namespace Lykke.Service.HFT.Services
{
    public class BlockedClientsService : IBlockedClientsService
    {
        private readonly ILog _log;
        private readonly IClientAccountClient _clientAccountClient;

        public BlockedClientsService(ILogFactory logFactory, IClientAccountClient clientAccountClient)
        {
            _log = logFactory.CreateLog(this);
            _clientAccountClient = clientAccountClient;
        }

        public async Task<bool> IsClientBlocked(string clientId)
        {
            try
            {
                return (await _clientAccountClient.ClientSettings.GetCashOutBlockSettingsAsync(clientId))?.TradesBlocked == true;
            } 
            catch(ClientApiException ex) when (ex.Message == "Client not found")
            {
                _log.Warning("Client not found. It will be treated as blocked one", context: new
                {
                    ClientId = clientId
                });

                return false;
            }
        }
    }
}
