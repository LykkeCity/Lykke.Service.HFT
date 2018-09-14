using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.HFT.Contracts.OrderBook;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Services
{
    public class OrderBookService : IOrderBooksService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IAssetPairsReadModel _assetPairsReadModel;

        public OrderBookService(
            IDistributedCache distributedCache,
            IAssetPairsReadModel assetPairsReadModel)
        {
            _distributedCache = distributedCache;
            _assetPairsReadModel = assetPairsReadModel;
        }

        public async Task<ICollection<Guid>> GetOrderIdsAsync(IEnumerable<string> assetPairs)
        {
            var tasks = assetPairs
                .SelectMany(x => new[]
                {
                    ( Pair: x, Buy: true),
                    ( Pair: x, Buy: false)
                })
                .Select(x => GetOrderIds(x.Pair, x.Buy));
            var results = await Task.WhenAll(tasks);

            return results.SelectMany(x => x).ToHashSet();
        }

        public async Task<IEnumerable<OrderBookModel>> GetAllAsync()
        {
            var assetPairs = _assetPairsReadModel.GetAll();
            var results = await Task.WhenAll(assetPairs.Select(pair => GetAsync(pair.Id)));

            return results.SelectMany(x => x).Where(x => x != null).ToList();
        }

        public async Task<IEnumerable<OrderBookModel>> GetAsync(string assetPairId)
        {
            if (string.IsNullOrWhiteSpace(assetPairId)) throw new ArgumentException(nameof(assetPairId));

            var buyBook = await GetOrderBook(assetPairId, true);
            var sellBook = await GetOrderBook(assetPairId, false);

            return new[] { sellBook, buyBook };
        }

        private async Task<OrderBookModel> GetOrderBook(string assetPair, bool buy)
        {
            var orderBook = await _distributedCache.GetStringAsync(Constants.GetKeyForOrderBook(assetPair, buy));
            return orderBook != null
                ? JsonConvert.DeserializeObject<OrderBookModel>(orderBook)
                : new OrderBookModel { AssetPair = assetPair, IsBuy = buy, Timestamp = DateTime.UtcNow };
        }

        private async Task<IEnumerable<Guid>> GetOrderIds(string assetPairId, bool buy)
        {
            var orderBook = await _distributedCache.GetStringAsync(Constants.GetKeyForOrderBook(assetPairId, buy));
            if (orderBook == null)
            {
                return Enumerable.Empty<Guid>();
            }

            return JsonConvert.DeserializeObject<OrderBookInternal>(orderBook)
                .Prices
                .Select(x => x.Id)
                .Where(x => Guid.TryParse(x, out Guid _))
                .Select(Guid.Parse);
        }
    }
}
