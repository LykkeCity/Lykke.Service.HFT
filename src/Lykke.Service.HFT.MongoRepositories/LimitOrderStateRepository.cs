using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using MongoDB.Driver;

namespace Lykke.Service.HFT.MongoRepositories
{
    public class LimitOrderStateRepository : MongoRepository<LimitOrderState>, ILimitOrderStateRepository
    {
        private readonly FilterDefinitionBuilder<LimitOrderState> _filterBuilder;
        private readonly SortDefinitionBuilder<LimitOrderState> _sortBuilder;

        public LimitOrderStateRepository(IMongoDatabase database, ILog log, int rpsLimit = 100) : base(database, log, rpsLimit)
        {
            _filterBuilder = new FilterDefinitionBuilder<LimitOrderState>();
            _sortBuilder = new SortDefinitionBuilder<LimitOrderState>();
        }

        public async Task<IEnumerable<LimitOrderState>> GetOrdersByStatus(string clientId, IEnumerable<OrderStatus> states, int take = 100, int skip = 0)
        {
            var filter = _filterBuilder.Where(x => x.ClientId == clientId);
            var inStates = states.ToList() ?? new List<OrderStatus>();

            if (inStates.Count != 0)
            {
                filter = _filterBuilder.And(
                    filter, 
                    _filterBuilder.In(x => x.Status, inStates));
            }

            var options = new FindOptions<LimitOrderState>
            {
                Limit = take,
                BatchSize = take,
                Skip = skip,
                Sort = _sortBuilder.Descending(x => x.CreatedAt)
            };

            var result = await GetCollection().FindAsync(filter, options);
            return result.ToEnumerable();
        }
    }
}