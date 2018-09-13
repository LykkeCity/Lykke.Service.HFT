using Autofac;
using JetBrains.Annotations;
using Lykke.Service.Assets.Contract.Events;
using System;

namespace AssetsCache
{
    /// <summary>
    /// Service registration for client asset services.
    /// </summary>
    [UsedImplicitly]
    public static class ContainerBuilderExtenions
    {
        /// <summary>
        /// Register the assets read model.
        /// </summary>
        /// <param name="builder">The container builder for adding the services to.</param>
        /// <param name="onCreated">Action for updating read model when Asset is created</param>
        /// <param name="onUpdated">Action for updating read model when Asset is updated</param>
        [UsedImplicitly]
        public static void RegisterAssetsReadModel(this ContainerBuilder builder,
            Action<AssetCreatedEvent> onCreated, Action<AssetUpdatedEvent> onUpdated)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterType<AssetsProjection>()
                .WithParameter(TypedParameter.From(onCreated))
                .WithParameter(TypedParameter.From(onUpdated));
        }

        /// <summary>
        /// Register the asset-pairs read model.
        /// </summary>
        /// <param name="builder">The container builder for adding the services to.</param>
        /// <param name="onCreated">Action for updating read model when Asset-pair is created</param>
        /// <param name="onUpdated">Action for updating read model when Asset-pair is updated</param>
        [UsedImplicitly]
        public static void RegisterAssetPairsReadModel(this ContainerBuilder builder,
            Action<AssetPairCreatedEvent> onCreated, Action<AssetPairUpdatedEvent> onUpdated)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterType<AssetPairsProjection>()
                .WithParameter(TypedParameter.From(onCreated))
                .WithParameter(TypedParameter.From(onUpdated));
        }
    }
}
