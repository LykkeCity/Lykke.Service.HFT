using Autofac;
using Autofac.Core;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.MongoRepositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Lykke.ApiKeyGenerator
{
    public class ServiceModule : Module
    {
        private readonly AppSettings _settings;
        private readonly AppSettings.HighFrequencyTradingSettings _serviceSettings;

        public ServiceModule(AppSettings settings)
        {
            _settings = settings;
            _serviceSettings = _settings.HighFrequencyTradingService;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings)
                .SingleInstance();
            builder.RegisterInstance(_serviceSettings)
                .SingleInstance();

            RegisterApiKeyService(builder);

            BindMongoDb(builder);
            BindRedis(builder);
        }

        private void BindRedis(ContainerBuilder builder)
        {
            var financeDataRedisCache = new RedisCache(new RedisCacheOptions
            {
                Configuration = _settings.HighFrequencyTradingService.CacheSettings.RedisConfiguration,
                InstanceName = _settings.HighFrequencyTradingService.CacheSettings.FinanceDataCacheInstance
            });
            builder.RegisterInstance(financeDataRedisCache)
                .As<IDistributedCache>()
                .Keyed<IDistributedCache>("financeData")
                .SingleInstance();

            var apiKeysRedisCache = new RedisCache(new RedisCacheOptions
            {
                Configuration = _settings.HighFrequencyTradingService.CacheSettings.RedisConfiguration,
                InstanceName = _settings.HighFrequencyTradingService.CacheSettings.ApiKeyCacheInstance
            });
            builder.RegisterInstance(apiKeysRedisCache)
                .As<IDistributedCache>()
                .Keyed<IDistributedCache>("apiKeys")
                .SingleInstance();
        }

        private void RegisterApiKeyService(ContainerBuilder builder)
        {
            builder.RegisterType<ApiKeyGenerator>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IDistributedCache),
                        (pi, ctx) => ctx.ResolveKeyed<IDistributedCache>("apiKeys")))
                .As<IApiKeyGenerator>()
                .SingleInstance();


            builder.RegisterType<MongoRepository<ApiKey>>()
                .As<IRepository<ApiKey>>()
                .SingleInstance();
        }

        private void BindMongoDb(ContainerBuilder builder)
        {
            var mongoUrl = new MongoUrl(_serviceSettings.MongoSettings.ConnectionString);
            ConventionRegistry.Register("Ignore extra", new ConventionPack { new IgnoreExtraElementsConvention(true) }, x => true);

            var database = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
            builder.RegisterInstance(database);
        }
    }
}
