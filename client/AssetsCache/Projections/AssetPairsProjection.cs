using System.Threading.Tasks;
using Lykke.Service.Assets.Contract.Events;
using Microsoft.Extensions.Caching.Memory;

namespace AssetsCache.Projections
{
    public class AssetPairsProjection
    {
        private readonly IMemoryCache _cache;

        public AssetPairsProjection(IMemoryCache cache)
        {
            _cache = cache;
        }

        private Task Handle(AssetPairCreatedEvent evt)
        {
            _cache.Set(evt.Id, Mapper.ToAssetPair(evt));
            return Task.CompletedTask;
        }

        public Task Handle(AssetPairUpdatedEvent evt)
        {
            _cache.Set(evt.Id, Mapper.ToAssetPair(evt));
            return Task.CompletedTask;
        }
    }
}
