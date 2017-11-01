using System;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.Assets.Client.Custom;
using Lykke.Service.HFT.AzureRepositories;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Accounts;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Lykke.Service.HFT.Core.Services.Assets;
using Lykke.Service.HFT.MongoRepositories;
using Lykke.Service.HFT.Services;
using Lykke.Service.HFT.Services.Assets;
using Lykke.SettingsReader;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.HFT.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly IServiceCollection _services;
        private readonly ILog _log;

        public ServiceModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            var currentSettings = _settings.CurrentValue;
            builder.RegisterInstance(currentSettings.Exchange)
                .SingleInstance();
            builder.RegisterInstance(currentSettings.HighFrequencyTradingService.CacheSettings)
                .SingleInstance();

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            RegisterApiKeyService(builder);

            RegisterBalances(builder, _settings.Nested(x => x.HighFrequencyTradingService.Db));

            RegisterOrderBooks(builder);

            RegisterAssets(builder, currentSettings.HighFrequencyTradingService.Dictionaries);

            RegisterOrderBookStates(builder);

            BindRedis(builder, currentSettings.HighFrequencyTradingService.CacheSettings);
            BindRabbitMq(builder, currentSettings.HighFrequencyTradingService.LimitOrdersFeed);

            builder.Populate(_services);
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

        private void RegisterBalances(ContainerBuilder builder, IReloadingManager<AppSettings.DbSettings> settings)
        {
            builder.RegisterInstance<IWalletsRepository>(
                AzureRepoFactories.CreateAccountsRepository(settings.Nested(x => x.BalancesInfoConnString), _log));
        }

        private void RegisterAssets(ContainerBuilder builder, AppSettings.DictionariesSettings settings)
        {
            _services.UseAssetsClient(AssetServiceSettings.Create(
                new Uri(settings.AssetsServiceUrl),
                settings.CacheExpirationPeriod));

            builder.RegisterType<AssetPairsManager>()
                .As<IAssetPairsManager>()
                .SingleInstance();
        }

        private void BindRabbitMq(ContainerBuilder builder, AppSettings.RabbitMqSettings settings)
        {
            builder.RegisterType<LimitOrdersConsumer>()
                .WithParameter(TypedParameter.From(settings))
                .SingleInstance().AutoActivate();
        }

        private void RegisterOrderBookStates(ContainerBuilder builder)
        {
            builder.RegisterType<MongoRepository<LimitOrderState>>()
                .As<IRepository<LimitOrderState>>()
                .SingleInstance();
        }

    }
}
