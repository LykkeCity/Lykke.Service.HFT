using System.Threading.Tasks;
using Lykke.Service.Assets.Contract.Events;
using Microsoft.Extensions.Caching.Memory;

namespace AssetsCache.Projections
{
    public class AssetsProjection
    {
        private readonly IMemoryCache _cache;

        public AssetsProjection(IMemoryCache cache)
        {
            _cache = cache;
        }

        private Task Handle(AssetCreatedEvent evt)
        {
            _cache.Set(evt.Id, Mapper.ToAsset(evt));
            return Task.CompletedTask;
        }

        public Task Handle(AssetUpdatedEvent evt)
        {
            _cache.Set(evt.Id, Mapper.ToAsset(evt));
            return Task.CompletedTask;
        }
    }
}
