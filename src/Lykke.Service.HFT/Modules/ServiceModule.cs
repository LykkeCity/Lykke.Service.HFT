using System;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using Lykke.MatchingEngine.Connector.Services;
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
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Lykke.Service.HFT.Modules
{
	public class ServiceModule : Module
	{
		private readonly IReloadingManager<AppSettings> _settings;
		private readonly AppSettings.HighFrequencyTradingSettings _serviceSettings;
		private readonly IServiceCollection _services;
		private readonly ILog _log;

		public ServiceModule(IReloadingManager<AppSettings> settings, ILog log)
		{
			_settings = settings;
			_serviceSettings = _settings.CurrentValue.HighFrequencyTradingService;
			_log = log;

			_services = new ServiceCollection();
		}

		protected override void Load(ContainerBuilder builder)
		{
		    builder.RegisterInstance(_settings)
		        .SingleInstance();
            builder.RegisterInstance(_serviceSettings)
				.SingleInstance();
		    builder.RegisterInstance(_settings.CurrentValue.Exchange)
		        .SingleInstance();
		    builder.RegisterInstance(_settings.CurrentValue.HighFrequencyTradingService.LimitOrdersFeed)
		        .SingleInstance();

            builder.RegisterInstance(_log)
				.As<ILog>()
				.SingleInstance();

			RegisterApiKeyService(builder);

			RegisterMatchingEngine(builder);

			RegisterBalances(builder);

			RegisterOrderBooks(builder);

			RegisterAssets(builder);

			RegisterOrderBookStates(builder);

            BindMongoDb(builder);
			BindRedis(builder);
			BindRabbitMq(builder);

			builder.Populate(_services);
		}

		private void BindRedis(ContainerBuilder builder)
		{
            var financeDataRedisCache = new RedisCache(new RedisCacheOptions
			{
				Configuration = _serviceSettings.CacheSettings.RedisConfiguration,
				InstanceName = _serviceSettings.CacheSettings.FinanceDataCacheInstance
			});
			builder.RegisterInstance(financeDataRedisCache)
				.As<IDistributedCache>()
				.Keyed<IDistributedCache>("financeData")
				.SingleInstance();

			var apiKeysRedisCache = new RedisCache(new RedisCacheOptions
			{
				Configuration = _serviceSettings.CacheSettings.RedisConfiguration,
				InstanceName = _serviceSettings.CacheSettings.ApiKeyCacheInstance
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

		private void RegisterMatchingEngine(ContainerBuilder builder)
		{
			var socketLog = new SocketLogDynamic(i => { },
				str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str));

			builder.BindMeClient(_settings.CurrentValue.MatchingEngineClient.IpEndpoint.GetClientIpEndPoint(), socketLog);

			builder.RegisterType<MatchingEngineAdapter>()
				.As<IMatchingEngineAdapter>()
				.SingleInstance();
		}

		private void RegisterBalances(ContainerBuilder builder)
		{
			builder.RegisterInstance<IWalletsRepository>(
				AzureRepoFactories.CreateAccountsRepository(_settings.Nested(x => x.HighFrequencyTradingService.Db.BalancesInfoConnString), _log));
		}

		private void RegisterAssets(ContainerBuilder builder)
		{
			_services.UseAssetsClient(AssetServiceSettings.Create(
				new Uri(_serviceSettings.Dictionaries.AssetsServiceUrl),
				_serviceSettings.Dictionaries.CacheExpirationPeriod));

			builder.RegisterType<AssetPairsManager>()
				.As<IAssetPairsManager>()
				.SingleInstance();
		}

		private void BindRabbitMq(ContainerBuilder builder)
		{
			builder.RegisterType<LimitOrdersConsumer>().SingleInstance().AutoActivate();
			builder.RegisterInstance(_serviceSettings.LimitOrdersFeed);
		}

	    private void BindMongoDb(ContainerBuilder builder)
	    {
	        var mongoUrl = new MongoUrl(_serviceSettings.MongoSettings.ConnectionString);
	        ConventionRegistry.Register("Ignore extra", new ConventionPack { new IgnoreExtraElementsConvention(true) }, x => true);

	        var database = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
	        builder.RegisterInstance(database);
	    }

        private void RegisterOrderBookStates(ContainerBuilder builder)
		{
            builder.RegisterType<MongoRepository<LimitOrderState>>()
				.As<IRepository<LimitOrderState>>()
				.SingleInstance();
		}

	}
}
