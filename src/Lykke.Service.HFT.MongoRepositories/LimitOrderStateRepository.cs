using System;
using System.Collections.Generic;
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

        public LimitOrderStateRepository(IMongoDatabase database, ILog log, int rpsLimit = 100) : base(database, log, rpsLimit)
        {
            _filterBuilder = new MongoDB.Driver.FilterDefinitionBuilder<LimitOrderState>();
        }

        public async Task<IEnumerable<LimitOrderState>> GetOrders(string clientId, QueryOrderStatus status, int take = 100, int skip = 0)
        {
            var filter = _filterBuilder.Where(x => x.ClientId == clientId);

            switch (status)
            {
                case QueryOrderStatus.All:
                    break;
                case QueryOrderStatus.Open:
                    {
                        var statusFilter = _filterBuilder.Where(x => x.Status == OrderStatus.InOrderBook
                                                                     || x.Status == OrderStatus.Processing);
                        filter = _filterBuilder.And(filter, statusFilter);
                        break;
                    }
                case QueryOrderStatus.InOrderBook:
                    {
                        var statusFilter = _filterBuilder.Where(x => x.Status == OrderStatus.InOrderBook);
                        filter = _filterBuilder.And(filter, statusFilter);
                        break;
                    }
                case QueryOrderStatus.Processing:
                    {
                        var statusFilter = _filterBuilder.Where(x => x.Status == OrderStatus.Processing);
                        filter = _filterBuilder.And(filter, statusFilter);
                        break;
                    }
                case QueryOrderStatus.Matched:
                    {
                        var statusFilter = _filterBuilder.Where(x => x.Status == OrderStatus.Matched);
                        filter = _filterBuilder.And(filter, statusFilter);
                        break;
                    }
                case QueryOrderStatus.Cancelled:
                    {
                        var statusFilter = _filterBuilder.Where(x => x.Status == OrderStatus.Cancelled);
                        filter = _filterBuilder.And(filter, statusFilter);
                        break;
                    }
                case QueryOrderStatus.Rejected:
                    {
                        var statusFilter = _filterBuilder.Where(x => x.Status != OrderStatus.Pending
                                                                     && x.Status != OrderStatus.InOrderBook
                                                                     && x.Status != OrderStatus.Processing
                                                                     && x.Status != OrderStatus.Matched
                                                                     && x.Status != OrderStatus.Cancelled);
                        filter = _filterBuilder.And(filter, statusFilter);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            var options = new FindOptions<LimitOrderState>
            {
                Limit = take,
                BatchSize = take,
                Skip = skip,
                Sort = new JsonSortDefinition<LimitOrderState>($"{{ {nameof(LimitOrderState.CreatedAt)} : -1 }}")
            };

            var result = await GetCollection().FindAsync(filter, options);
            return result.ToEnumerable();
        }
    }
}