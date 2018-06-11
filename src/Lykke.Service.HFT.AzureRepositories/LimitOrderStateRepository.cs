using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;

namespace Lykke.Service.HFT.AzureRepositories
{
    public class LimitOrderStateArchive : ILimitOrderStateArchive
    {
        private readonly INoSQLTableStorage<LimitOrderStateEntity> _orderStateTable;

        public LimitOrderStateArchive(INoSQLTableStorage<LimitOrderStateEntity> orderStateTable)
        {
            _orderStateTable = orderStateTable;
        }

        public async Task<ILimitOrderState> GetAsync(string clientId, Guid orderId)
        {
            return await _orderStateTable.GetDataAsync(clientId, orderId.ToString());
        }

        public async Task AddAsync(IEnumerable<ILimitOrderState> orders)
        {
            var chunks = orders.GroupBy(GetPartitionKey);
            await chunks.ParallelForEachAsync(async chunk =>
            {
                await _orderStateTable.InsertOrReplaceBatchAsync(chunk.Select(Create));
            });
        }

        private LimitOrderStateEntity Create(ILimitOrderState order)
        {
            return new LimitOrderStateEntity
            {
                PartitionKey = GetPartitionKey(order),
                RowKey = GetRowKey(order),
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

        private static string GetPartitionKey(ILimitOrderState order)
            => order.ClientId;

        private static string GetRowKey(ILimitOrderState order)
            => order.Id.ToString();
    }
}
