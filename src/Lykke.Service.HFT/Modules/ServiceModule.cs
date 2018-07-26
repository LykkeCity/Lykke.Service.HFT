using System;
using Autofac;
using Autofac.Core;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Sdk;
using Lykke.Service.HFT.AzureRepositories;
using Lykke.Service.HFT.Controllers;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Lykke.Service.HFT.MongoRepositories;
using Lykke.Service.HFT.PeriodicalHandlers;
using Lykke.Service.HFT.Services;
using Lykke.Service.HFT.Services.Consumers;
using Lykke.Service.HFT.Services.Fees;
using Lykke.SettingsReader;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using MongoDB.Driver;

namespace Lykke.Service.HFT.Modules
{
    public class ServiceModule : Module
    {
        private const string FinanceDataCache = "financeData";
        private const string ApiKeysCache = "apiKeys";
        private readonly IReloadingManager<AppSettings> _settings;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var currentSettings = _settings.CurrentValue;
            builder.RegisterInstance(currentSettings.HighFrequencyTradingService.CacheSettings)
                .SingleInstance();
            builder.RegisterInstance(currentSettings.HighFrequencyTradingService)
                .SingleInstance();

            if (currentSettings.HighFrequencyTradingService.MaintenanceMode != null)
            {
                builder.RegisterInstance(currentSettings.HighFrequencyTradingService.MaintenanceMode)
                    .AsSelf();
            }

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance();

            builder.RegisterType<RequestValidator>()
                .SingleInstance();

            RegisterApiKeyService(builder);

            RegisterFeeServices(builder);

            RegisterOrderBooks(builder);

            RegisterAssets(builder);

            RegisterOrderStates(builder);

            BindRedis(builder, currentSettings.HighFrequencyTradingService.CacheSettings);
            BindRabbitMq(builder, currentSettings.HighFrequencyTradingService);

            RegisterPeriodicalHandlers(builder);
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
                .Keyed<IDistributedCache>(FinanceDataCache)
                .SingleInstance();

            var apiKeysRedisCache = new RedisCache(new RedisCacheOptions
            {
                Configuration = settings.RedisConfiguration,
                InstanceName = settings.ApiKeyCacheInstance
            });
            builder.RegisterInstance(apiKeysRedisCache)
                .As<IDistributedCache>()
                .Keyed<IDistributedCache>(ApiKeysCache)
                .SingleInstance();
        }

        private void RegisterApiKeyService(ContainerBuilder builder)
        {
            builder.RegisterType<HftClientService>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>(ApiKeysCache)))
                .As<IHftClientService>()
                .SingleInstance();

            builder.RegisterType<CachedSessionRepository>()
                .As<ISessionRepository>()
                .SingleInstance();

            builder.RegisterType<ApiKeyValidator>()
                .As<IApiKeyValidator>()
                .SingleInstance();

            builder.RegisterType<ApiKeyCacheInitializer>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>(ApiKeysCache)))
                .As<IApiKeyCacheInitializer>()
                .SingleInstance();

            builder.RegisterType<MongoRepository<ApiKey>>()
                .As<IRepository<ApiKey>>()
                .SingleInstance();
        }

        private void RegisterFeeServices(ContainerBuilder builder)
        {
            builder.RegisterType<FeeCalculatorAdapter>()
                .As<IFeeCalculatorAdapter>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.FeeSettings))
                .SingleInstance();
        }

        private void RegisterOrderBooks(ContainerBuilder builder)
        {
            builder.RegisterType<OrderBookService>()
                .As<IOrderBooksService>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>(FinanceDataCache)))
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
        }

        private void RegisterOrderStates(ContainerBuilder builder)
        {
            var mongoUrl = new MongoUrl(_settings.CurrentValue.HighFrequencyTradingService.Db.OrderStateConnString);
            builder.RegisterType<LimitOrderStateRepository>()
                .As<IRepository<LimitOrderState>>()
                .As<ILimitOrderStateRepository>()
                .WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(IMongoDatabase),
                    (pi, ctx) => new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName)))
                .SingleInstance();

            builder.RegisterType<LimitOrderStateArchive>()
                .As<ILimitOrderStateArchive>()
                .WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(INoSQLTableStorage<LimitOrderStateEntity>),
                    (pi, ctx) => AzureTableStorage<LimitOrderStateEntity>.Create(
                        _settings.ConnectionString(x => x.HighFrequencyTradingService.Db.OrdersArchiveConnString),
                        "HftOrderStateArchive",
                        ctx.Resolve<ILogFactory>())))
                .SingleInstance();
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<GhostOrdersRemover>()
                .WithParameter(TypedParameter.From(TimeSpan.FromHours(4)))
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<OrderStateArchiver>()
                .WithParameter("checkInterval", TimeSpan.FromHours(12))
                .WithParameter("activeOrdersWindow", TimeSpan.FromDays(30))
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<PendingOrdersChecker>()
                .WithParameter(TypedParameter.From(TimeSpan.FromMinutes(5)))
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}
