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
using Lykke.Service.HFT.Core.Settings;
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
    internal class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var currentSettings = _settings.CurrentValue;
            builder.RegisterInstance(currentSettings.HighFrequencyTradingService.Cache)
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

            BindRabbitMq(builder, currentSettings.HighFrequencyTradingService);

            RegisterPeriodicalHandlers(builder);
        }

        private void RegisterApiKeyService(ContainerBuilder builder)
        {
            RegisterRedisCache(builder, Constants.ApiKeyCacheInstance);

            builder.RegisterType<HftClientService>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>(Constants.ApiKeyCacheInstance)))
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
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>(Constants.ApiKeyCacheInstance)))
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
            RegisterRedisCache(builder, Constants.FinanceDataCacheInstance);

            builder.RegisterType<OrderBookService>()
                .As<IOrderBooksService>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>(Constants.FinanceDataCacheInstance)))
                .SingleInstance();
        }

        private void RegisterAssets(ContainerBuilder builder)
        {
            builder.RegisterType<AssetServiceDecorator>()
                .As<IAssetServiceDecorator>()
                .SingleInstance();
        }

        private void BindRabbitMq(ContainerBuilder builder, HighFrequencyTradingSettings settings)
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

        private void RegisterRedisCache(ContainerBuilder builder, string name)
        {
            var settings = _settings.CurrentValue.HighFrequencyTradingService.Cache;
            var cache = new RedisCache(new RedisCacheOptions
            {
                Configuration = settings.RedisConfiguration,
                InstanceName = name
            });
            builder.RegisterInstance(cache)
                .As<IDistributedCache>()
                .Keyed<IDistributedCache>(name)
                .SingleInstance();
        }
    }
}
