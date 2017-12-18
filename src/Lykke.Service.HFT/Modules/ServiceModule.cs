using Autofac;
using Autofac.Core;
using Common.Log;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Lykke.Service.HFT.MongoRepositories;
using Lykke.Service.HFT.Services;
using Lykke.Service.HFT.Services.Consumers;
using Lykke.SettingsReader;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;

namespace Lykke.Service.HFT.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public ServiceModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var currentSettings = _settings.CurrentValue;
            builder.RegisterInstance(currentSettings.Exchange)
                .SingleInstance();
            builder.RegisterInstance(currentSettings.HighFrequencyTradingService.CacheSettings)
                .SingleInstance();
            builder.RegisterInstance(currentSettings.HighFrequencyTradingService)
                .SingleInstance();

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            RegisterApiKeyService(builder);

            RegisterOrderBooks(builder);

            RegisterAssets(builder);

            RegisterOrderStates(builder);

            BindRedis(builder, currentSettings.HighFrequencyTradingService.CacheSettings);
            BindRabbitMq(builder, currentSettings.HighFrequencyTradingService);
        }

        private void BindRedis(ContainerBuilder builder, CacheSettings settings)
        {
            var financeDataRedisCache = new RedisCache(new RedisCacheOptions
            {
                Configuration = settings.RedisConfiguration,
                InstanceName = settings.FinanceDataCacheInstance
            });
            builder.RegisterInstance(financeDataRedisCache)
                .As<IDistributedCache>()
                .Keyed<IDistributedCache>("financeData")
                .SingleInstance();

            var apiKeysRedisCache = new RedisCache(new RedisCacheOptions
            {
                Configuration = settings.RedisConfiguration,
                InstanceName = settings.ApiKeyCacheInstance
            });
            builder.RegisterInstance(apiKeysRedisCache)
                .As<IDistributedCache>()
                .Keyed<IDistributedCache>("apiKeys")
                .SingleInstance();
        }

        private void RegisterApiKeyService(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<ApiKeyService>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>("apiKeys")))
                .As<IApiKeyValidator>()
                .As<IClientResolver>()
                .SingleInstance();

            builder.RegisterType<ApiKeyCacheInitializer>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>("apiKeys")))
                .As<IApiKeyCacheInitializer>()
                .SingleInstance();


            builder.RegisterType<MongoRepository<ApiKey>>()
                .As<IRepository<ApiKey>>()
                .SingleInstance();
        }

        private void RegisterOrderBooks(ContainerBuilder builder)
        {
            builder.RegisterType<OrderBookService>()
                .As<IOrderBooksService>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>("financeData")))
                .SingleInstance();
        }

        private void RegisterAssets(ContainerBuilder builder)
        {
            builder.RegisterType<AssetServiceDecorator>()
                .As<IAssetServiceDecorator>()
                .SingleInstance();
        }

        private void BindRabbitMq(ContainerBuilder builder, AppSettings.HighFrequencyTradingSettings settings)
        {
            builder.RegisterType<LimitOrdersConsumer>()
                .WithParameter(TypedParameter.From(settings.LimitOrdersFeed))
                .SingleInstance().AutoActivate();
            builder.RegisterType<ApiKeysConsumer>()
                .WithParameter(TypedParameter.From(settings.ApiKeysFeed))
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>("apiKeys")))
                .SingleInstance().AutoActivate();
        }

        private void RegisterOrderStates(ContainerBuilder builder)
        {
            builder.RegisterType<MongoRepository<LimitOrderState>>()
                .As<IRepository<LimitOrderState>>()
                .SingleInstance();
        }
    }
}
