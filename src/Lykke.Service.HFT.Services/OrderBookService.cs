using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services
{
	public class OrderBookService : IOrderBooksService
	{
		private readonly IDistributedCache _distributedCache;
		private readonly CachedDataDictionary<string, IAssetPair> _assetPairsDict;
		private readonly HighFrequencyTradingSettings _settings;

		public OrderBookService(
			IDistributedCache distributedCache,
			CachedDataDictionary<string, IAssetPair> assetPairsDict,
			HighFrequencyTradingSettings settings)
		{
			_distributedCache = distributedCache;
			_assetPairsDict = assetPairsDict;
			_settings = settings;
		}

		public async Task<IEnumerable<IOrderBook>> GetAllAsync()
		{
			var assetPairs = await _assetPairsDict.Values();

			var orderBooks = new List<OrderBook>();

			foreach (var pair in assetPairs)
			{
				var buyBookJson = _distributedCache.GetStringAsync(_settings.CacheSettings.GetOrderBookKey(pair.Id, true));
				var sellBookJson = _distributedCache.GetStringAsync(_settings.CacheSettings.GetOrderBookKey(pair.Id, false));

				var buyBook = (await buyBookJson)?.DeserializeJson<OrderBook>();

				if (buyBook != null)
					orderBooks.Add(buyBook);

				var sellBook = (await sellBookJson)?.DeserializeJson<OrderBook>();

				if (sellBook != null)
					orderBooks.Add(sellBook);
			}

			return orderBooks;
		}

		public async Task<IEnumerable<IOrderBook>> GetAsync(string assetPairId)
		{
			if (string.IsNullOrWhiteSpace(assetPairId)) throw new ArgumentException(nameof(assetPairId));

			var sellBookTask = _distributedCache.GetStringAsync(_settings.CacheSettings.GetOrderBookKey(assetPairId, false));
			var buyBookTask = _distributedCache.GetStringAsync(_settings.CacheSettings.GetOrderBookKey(assetPairId, true));

			var sellBookStr = await sellBookTask;
			var sellBook = sellBookStr != null ? sellBookStr.DeserializeJson<OrderBook>() :
				new OrderBook { AssetPair = assetPairId, IsBuy = false, Timestamp = DateTime.UtcNow };

			var buyBookStr = await buyBookTask;
			var buyBook = buyBookStr != null ? buyBookStr.DeserializeJson<OrderBook>() :
				new OrderBook { AssetPair = assetPairId, IsBuy = true, Timestamp = DateTime.UtcNow };

			return new IOrderBook[] { sellBook, buyBook };
		}
	}
}
