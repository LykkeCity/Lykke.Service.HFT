using System;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;

namespace Lykke.Service.HFT.AzureRepositories
{
    public class LimitOrderStateRepository : ILimitOrderStateRepository
    {
        private readonly INoSQLTableStorage<LimitOrderStateEntity> _orderStateTable;

        public LimitOrderStateRepository(
            INoSQLTableStorage<LimitOrderStateEntity> orderStateTable)
        {
            _orderStateTable = orderStateTable;
        }

        public async Task<ILimitOrderState> GetAsync(string clientId, Guid orderId)
        {
            return await _orderStateTable.GetDataAsync(clientId, orderId.ToString());
        }
    }
}
