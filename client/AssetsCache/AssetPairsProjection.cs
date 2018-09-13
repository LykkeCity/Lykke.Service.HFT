using System;
using System.Threading.Tasks;
using Lykke.Service.Assets.Contract.Events;

namespace AssetsCache
{
    public class AssetPairsProjection
    {
        private readonly Action<AssetPairCreatedEvent> _onCreated;
        private readonly Action<AssetPairUpdatedEvent> _onUpdated;

        public AssetPairsProjection(Action<AssetPairCreatedEvent> onCreated, Action<AssetPairUpdatedEvent> onUpdated)
        {
            _onCreated = onCreated;
            _onUpdated = onUpdated;
        }

        private async Task Handle(AssetPairCreatedEvent evt)
        {
            _onCreated(evt);
        }

        public async Task Handle(AssetPairUpdatedEvent evt)
        {
            _onUpdated(evt);
        }
    }
}
