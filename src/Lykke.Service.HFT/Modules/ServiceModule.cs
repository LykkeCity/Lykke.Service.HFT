using System;
using Autofac;
using Autofac.Core;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
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
using Lykke.SettingsReader;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.WindowsAzure.Storage.Table;
using MongoDB.Driver;

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
            builder.RegisterInstance(currentSettings.HighFrequencyTradingService.CacheSettings)
                .SingleInstance();
            builder.RegisterInstance(currentSettings.HighFrequencyTradingService)
                .SingleInstance();

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            if (currentSettings.HighFrequencyTradingService.MaintenanceMode != null)
            {
                builder.RegisterInstance(currentSettings.HighFrequencyTradingService.MaintenanceMode)
                    .AsSelf();
            }

            builder.RegisterType<RequestValidator>()
                .SingleInstance();

            RegisterApiKeyService(builder);

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

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<HftClientService>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>("apiKeys")))
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
        }

        private void RegisterOrderStates(ContainerBuilder builder)
        {
            var mongoUrl = new MongoUrl(_settings.CurrentValue.HighFrequencyTradingService.Db.OrderStateConnString);
            var database = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
            builder.RegisterInstance(new MongoRepository<LimitOrderState>(database, _log))
                .As<IRepository<LimitOrderState>>()
                .SingleInstance();

            var ordersArchiveTable = CreateTable<LimitOrderStateEntity>(
                _settings.ConnectionString(x => x.HighFrequencyTradingService.Db.OrdersArchiveConnString), "HftOrderStateArchive");
            builder.RegisterInstance(new LimitOrderStateRepository(ordersArchiveTable))
                .As<ILimitOrderStateRepository>()
                .SingleInstance();
        }

        private INoSQLTableStorage<T> CreateTable<T>(IReloadingManager<string> connectionString, string name)
            where T : TableEntity, new()
        {
            return AzureTableStorage<T>.Create(connectionString, name, _log);
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
        }
    }
}
