using AssetsCache.Projections;
using AssetsCache.ReadModels;
using Autofac;
using JetBrains.Annotations;
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
        /// Register the default in-memory assets and asset-pairs read model.
        /// </summary>
        /// <param name="builder">The container builder for adding the services to.</param>
        [UsedImplicitly]
        public static void RegisterDefaultAssetsReadModel(this ContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterType<AssetsReadModel>()
                .As<IAssetsReadModel>()
                .As<IStartable>()
                .AutoActivate();
            builder.RegisterType<AssetPairsReadModel>()
                .As<IAssetPairsReadModel>()
                .As<IStartable>()
                .AutoActivate();

            builder.RegisterType<AssetsProjection>();
            builder.RegisterType<AssetPairsProjection>();
        }
    }
}
