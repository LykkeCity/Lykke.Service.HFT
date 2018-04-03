using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task AddAsync(IEnumerable<ILimitOrderState> orders)
        {
            return _orderStateTable.InsertOrReplaceBatchAsync(orders.Select(Create));
        }

        private LimitOrderStateEntity Create(ILimitOrderState order)
        {
            return new LimitOrderStateEntity
            {
                PartitionKey = order.ClientId,
                RowKey = order.Id.ToString(),
                Status = order.Status,
                Price = order.Price,
                AssetPairId = order.AssetPairId,
                Volume = order.Volume,
                RemainingVolume = order.RemainingVolume,
                ClientId = order.ClientId,
                LastMatchTime = order.LastMatchTime,
                CreatedAt = order.CreatedAt,
                Registered = order.Registered
            };
        }
    }
}
