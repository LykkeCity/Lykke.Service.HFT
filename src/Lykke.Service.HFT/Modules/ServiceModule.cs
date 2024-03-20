using Autofac;
using Autofac.Core;
using Lykke.Sdk;
using Lykke.Service.HFT.Controllers;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Lykke.Service.HFT.Core.Settings;
using Lykke.Service.HFT.MongoRepositories;
using Lykke.Service.HFT.Services;
using Lykke.Service.HFT.Services.Fees;
using Lykke.SettingsReader;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;

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
        }

        private void RegisterApiKeyService(ContainerBuilder builder)
        {
            var instanceName = _settings.CurrentValue.HighFrequencyTradingService.Cache.ApiKeyCacheInstance;
            RegisterRedisCache(builder, instanceName);

            builder.RegisterType<ApiKeysCacheService>()
                .WithParameter(TypedParameter.From(instanceName))
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>(instanceName)))
                .As<IApiKeysCacheService>()
                .SingleInstance();

            builder.RegisterType<CachedSessionRepository>()
                .As<ISessionRepository>()
                .SingleInstance();

            builder.RegisterType<ApiKeyValidator>()
                .As<IApiKeyValidator>()
                .SingleInstance();

            builder.RegisterType<ApiKeyCacheInitializer>()
                .As<IApiKeyCacheInitializer>()
                .SingleInstance();

            builder.RegisterType<MongoRepository<ApiKey>>()
                .As<IRepository<ApiKey>>()
                .SingleInstance();

            builder.RegisterType<BlockedClientsService>()
                .As<IBlockedClientsService>();
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
            var instanceName = _settings.CurrentValue.HighFrequencyTradingService.Cache.OrderBooksCacheInstance;
            RegisterRedisCache(builder, instanceName);

            builder.RegisterType<OrderBookService>()
                .As<IOrderBooksService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.HighFrequencyTradingService.Cache.OrderBooksCacheKeyPattern))
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>(instanceName)))
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
