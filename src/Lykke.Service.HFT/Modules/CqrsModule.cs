using System.Collections.Generic;
using AssetsCache;
using Autofac;
using Autofac.Core;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Settings;
using Lykke.Service.HFT.Services.Events;
using Lykke.Service.HFT.Services.Projections;
using Lykke.SettingsReader;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.HFT.Modules
{
    internal class CqrsModule : Module
    {
        private readonly HighFrequencyTradingSettings _settings;

        public CqrsModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settings = settingsManager.CurrentValue.HighFrequencyTradingService;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (_settings.ChaosKitty != null)
            {
                builder
                    .RegisterType<ChaosKitty>()
                    .WithParameter(TypedParameter.From(_settings.ChaosKitty.StateOfChaos))
                    .As<IChaosKitty>()
                    .SingleInstance();
            }
            else
            {
                builder
                    .RegisterType<SilentChaosKitty>()
                    .As<IChaosKitty>()
                    .SingleInstance();
            }

            MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CqrsRabbitConnString };

            var cache = new MemoryCache(new MemoryCacheOptions());
            builder.Register(ctx => cache).As<IMemoryCache>().SingleInstance();
            builder.RegisterAssetsReadModel(
                created => cache.Set(created.Id, AutoMapper.Mapper.Map<Asset>(created)),
                updated => cache.Set(updated.Id, AutoMapper.Mapper.Map<Asset>(updated)));
            builder.RegisterAssetPairsReadModel(
                created => cache.Set(created.Id, AutoMapper.Mapper.Map<AssetPair>(created)),
                updated => cache.Set(updated.Id, AutoMapper.Mapper.Map<AssetPair>(updated)));

            builder.RegisterType<ApiKeyProjection>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>(Constants.ApiKeyCacheInstance)));

            builder.Register(ctx =>
            {
                var logFactory = ctx.Resolve<ILogFactory>();
#if DEBUG
                var broker = rabbitMqSettings.Endpoint + "/debug";
#else
                var broker = rabbitMqSettings.Endpoint.ToString();
#endif
                var messagingEngine = new MessagingEngine(logFactory,
                    new TransportResolver(new Dictionary<string, TransportInfo>
                    {
                        {"RabbitMq", new TransportInfo(broker, rabbitMqSettings.UserName, rabbitMqSettings.Password, "None", "RabbitMq")}
                    }),
                    new RabbitMqTransportFactory(logFactory));

                const string defaultRoute = "self";

                return new CqrsEngine(logFactory,
                    ctx.Resolve<IDependencyResolver>(),
                    messagingEngine,
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                        "RabbitMq",
                        SerializationFormat.MessagePack,
                        environment: "lykke",
                        exclusiveQueuePostfix: _settings.QueuePostfix)),

                Register.BoundedContext("hft-api")
                    .ListeningEvents(
                        typeof(ApiKeyUpdatedEvent))
                    .From("api-key").On(defaultRoute)
                    .WithProjection(typeof(ApiKeyProjection), "api-key")
                );
            })
            .As<ICqrsEngine>().SingleInstance();
        }
    }
}
