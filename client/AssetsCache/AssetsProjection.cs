using System;
using System.Threading.Tasks;
using Lykke.Service.Assets.Contract.Events;

namespace AssetsCache
{
    public class AssetsProjection
    {
        private readonly Action<AssetCreatedEvent> _onCreated;
        private readonly Action<AssetUpdatedEvent> _onUpdated;

        public AssetsProjection(Action<AssetCreatedEvent> onCreated, Action<AssetUpdatedEvent> onUpdated)
        {
            _onCreated = onCreated;
            _onUpdated = onUpdated;
        }

        private async Task Handle(AssetCreatedEvent evt)
        {
            _onCreated(evt);
        }

        public async Task Handle(AssetUpdatedEvent evt)
        {
            _onUpdated(evt);
        }
    }
}
