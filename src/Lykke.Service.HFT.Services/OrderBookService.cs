﻿using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.HFT.Contracts.OrderBook;

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
            var assetPairs = await _assetServiceDecorator.GetAllEnabledAssetPairsAsync();
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
            var orderBook = await _distributedCache.GetStringAsync(_settings.GetKeyForOrderBook(assetPair, buy));
            return orderBook != null
                ? JsonConvert.DeserializeObject<OrderBookModel>(orderBook)
                : new OrderBookModel { AssetPair = assetPair, IsBuy = buy, Timestamp = DateTime.UtcNow };
        }

        private async Task<IEnumerable<Guid>> GetOrderIds(string assetPairId, bool buy)
        {
            var orderBook = await _distributedCache.GetStringAsync(_settings.GetKeyForOrderBook(assetPairId, buy));
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
