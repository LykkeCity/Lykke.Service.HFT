using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.Assets;
using Microsoft.Extensions.Caching.Distributed;

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
            var orderBook = await _distributedCache.GetStringAsync(_settings.GetOrderBookKey(assetPair, buy));
            return orderBook != null ? NetJSON.NetJSON.Deserialize<OrderBook>(orderBook) :
                new OrderBook { AssetPair = assetPair, IsBuy = buy, Timestamp = DateTime.UtcNow };
        }

        public async Task<double?> GetBestPrice(string assetPair, bool buy)
        {
            var orderBook = await GetOrderBook(assetPair, buy);

            var price = GetBestPrice(orderBook);
            if (price.HasValue && price.Value > 0)
                return price;

            orderBook = await GetOrderBook(assetPair, !buy);
            price = GetBestPrice(orderBook);
            return price;
        }

        private double? GetBestPrice(OrderBook orderBook)
        {
            if (orderBook.Prices.Count == 0)
                return null;
            return orderBook.IsBuy ? orderBook.Prices.Min(x => x.Price) : orderBook.Prices.Max(x => x.Price);
        }
    }
}
