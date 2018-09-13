using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Configuration.BoundedContext;
using Lykke.Service.Assets.Contract.Events;

namespace AssetsCache
{
    public static class Registration
    {
        public static IBoundedContextRegistration ReadModelForAssets(this IBoundedContextRegistration bcr)
        {
            const string assetsContextName = "assets";
            const string defaultRoute = "self";

            return bcr
                .ListeningEvents(typeof(AssetCreatedEvent), typeof(AssetUpdatedEvent))
                    .From(assetsContextName).On(defaultRoute)
                    .WithProjection(typeof(AssetsProjection), assetsContextName);
        }
        public static IBoundedContextRegistration ReadModelForAssetPairs(this IBoundedContextRegistration bcr)
        {
            const string assetsContextName = "assets";
            const string defaultRoute = "self";

            return bcr
                .ListeningEvents(typeof(AssetPairCreatedEvent), typeof(AssetPairUpdatedEvent))
                    .From(assetsContextName).On(defaultRoute)
                    .WithProjection(typeof(AssetPairsProjection), assetsContextName);
        }
    }
}
