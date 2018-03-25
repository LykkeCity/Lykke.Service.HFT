using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Lykke.Service.HFT.Services
{
    public class OrderBookService : IOrderBooksService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IAssetServiceDecorator _assetServiceDecorator;
        private readonly CacheSettings _settings;

        public OrderBookService(
            IDistributedCache distributedCache,
            IAssetServiceDecorator assetServiceDecorator,
            CacheSettings settings)
        {
            _distributedCache = distributedCache;
            _assetServiceDecorator = assetServiceDecorator ?? throw new ArgumentNullException(nameof(assetServiceDecorator));
            _settings = settings;
        }

        public async Task<ICollection<Guid>> GetOrderIdsAsync(IEnumerable<string> assetPairs)
        {
            var orderBooks = new List<string>();
            foreach (var pair in assetPairs)
            {
                var orderBook = await _distributedCache.GetStringAsync(_settings.GetKeyForOrderBook(pair, true));
                if (orderBook != null)
                    orderBooks.AddRange(JsonConvert.DeserializeObject<OrderBookInternal>(orderBook).Prices.Select(x => x.Id));

                orderBook = await _distributedCache.GetStringAsync(_settings.GetKeyForOrderBook(pair, false));
                if (orderBook != null)
                    orderBooks.AddRange(JsonConvert.DeserializeObject<OrderBookInternal>(orderBook).Prices.Select(x => x.Id));
            }

            return orderBooks
                .Where(x => Guid.TryParse(x, out Guid _))
                .Select(Guid.Parse)
                .ToHashSet();
        }

        public async Task<IEnumerable<OrderBook>> GetAllAsync()
        {
            var assetPairs = await _assetServiceDecorator.GetAllEnabledAssetPairsAsync();
            var orderBooks = new List<OrderBook>();
            foreach (var pair in assetPairs)
            {
                var buyBook = await GetOrderBook(pair.Id, true);
                if (buyBook != null)
                    orderBooks.Add(buyBook);

                var sellBook = await GetOrderBook(pair.Id, false);
                if (sellBook != null)
                    orderBooks.Add(sellBook);
            }

            return orderBooks;
        }

        public async Task<IEnumerable<OrderBook>> GetAsync(string assetPairId)
        {
            if (string.IsNullOrWhiteSpace(assetPairId)) throw new ArgumentException(nameof(assetPairId));

            var buyBook = await GetOrderBook(assetPairId, true);
            var sellBook = await GetOrderBook(assetPairId, false);

            return new[] { sellBook, buyBook };
        }

        private async Task<OrderBook> GetOrderBook(string assetPair, bool buy)
        {
            var orderBook = await _distributedCache.GetStringAsync(_settings.GetKeyForOrderBook(assetPair, buy));
            return orderBook != null ? JsonConvert.DeserializeObject<OrderBook>(orderBook) :
                new OrderBook { AssetPair = assetPair, IsBuy = buy, Timestamp = DateTime.UtcNow };
        }
    }
}
