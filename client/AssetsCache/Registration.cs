using AssetsCache.Projections;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Configuration.BoundedContext;
using Lykke.Service.Assets.Contract.Events;

namespace AssetsCache
{
    public static class Registration
    {
        public static IBoundedContextRegistration WithAssetsReadModel(this IBoundedContextRegistration bcr)
        {
            const string assetsContextName = "assets";
            const string assetPairsContextName = "assets";
            const string defaultRoute = "self";

            return bcr
                .ListeningEvents(typeof(AssetCreatedEvent), typeof(AssetUpdatedEvent))
                    .From(assetsContextName).On(defaultRoute)
                    .WithProjection(typeof(AssetsProjection), assetsContextName)
                .ListeningEvents(typeof(AssetPairCreatedEvent), typeof(AssetPairUpdatedEvent))
                    .From(assetPairsContextName).On(defaultRoute)
                    .WithProjection(typeof(AssetPairsProjection), assetPairsContextName);
        }
    }
}
