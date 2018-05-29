using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Core;
using MessagePack;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using OrderAction = Lykke.Service.HFT.Core.Domain.OrderAction;

namespace Lykke.Service.HFT.Services.Fees
{
    /// <summary>
    /// Cache for limit order fees. Used to speedup fee calculation for bots who trade the same asset-pair over and over.
    /// </summary>
    /// <remarks>
    /// At the moment doesn't make sence to cache market order fees since the fee is dependant on target wallet id which is variable for each transaction.
    /// </remarks>
    public class LimitOrderFeeCache
    {
        private readonly IDistributedCache _cache;
        private readonly FeeSettings _feeSettings;
        private readonly CacheSettings _cacheSettings;
        private readonly DistributedCacheEntryOptions _options;

        public LimitOrderFeeCache(IDistributedCache cache, FeeSettings feeSettings, CacheSettings cacheSettings)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _feeSettings = feeSettings ?? throw new ArgumentNullException(nameof(feeSettings));
            _cacheSettings = cacheSettings ?? throw new ArgumentNullException(nameof(cacheSettings));

            if (!TimeSpan.TryParse(cacheSettings.FeeCacheDuration, out var expiration))
            {
                expiration = TimeSpan.FromMinutes(10);
            }

            _options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
        }

        public async Task<LimitOrderFeeModel> TryGetLimitOrderFee(string clientId, AssetPair assetPair, OrderAction orderAction)
        {
            var key = GetKey(clientId, assetPair.Id, orderAction == OrderAction.Buy);
            var data = await _cache.GetAsync(key);
            if (data == null)
            {
                return null;
            }

            try
            {
                var entry = MessagePackSerializer.Deserialize<LimitOrderFeeCacheEntry>(data);

                return new LimitOrderFeeModel
                {
                    MakerSize = entry.MakerSize,
                    TakerSize = entry.TakerSize,
                    SourceClientId = clientId,
                    TargetClientId = _feeSettings.TargetClientId.Hft,
                    Type = entry.Type,
                    MakerFeeModificator = entry.MakerFeeModificator,
                    MakerSizeType = entry.MakerSizeType,
                    TakerSizeType = entry.TakerSizeType
                };
            }
            catch
            {
                return null;
            }
        }

        public void CacheLimitOrderFee(string clientId, AssetPair assetPair, OrderAction orderAction, LimitOrderFeeModel model)
        {
            var key = GetKey(clientId, assetPair.Id, orderAction == OrderAction.Buy);

            var entry = new LimitOrderFeeCacheEntry
            {
                Type = model.Type,
                MakerSize = model.MakerSize,
                TakerSize = model.TakerSize,
                MakerFeeModificator = model.MakerFeeModificator,
                MakerSizeType = model.MakerSizeType,
                TakerSizeType = model.TakerSizeType
            };

            var data = MessagePackSerializer.Serialize(entry);

            // don't await so caller wont be blocked by caching of data
            _cache.SetAsync(key, data, _options);
        }

        private string GetKey(string clientId, string assetPair, bool buy)
            => String.Format(_cacheSettings.FeeCacheLimitOrderPattern, clientId, assetPair, buy ? "B" : "S");
    }
}