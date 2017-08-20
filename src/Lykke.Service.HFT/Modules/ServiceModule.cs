using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.Assets.Client.Custom;
using Lykke.Service.HFT.AzureRepositories;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Accounts;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Lykke.Service.HFT.Core.Services.Assets;
using Lykke.Service.HFT.Services;
using Lykke.Service.HFT.Services.Assets;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.HFT.Modules
{
	public class ServiceModule : Module
	{
		private readonly AppSettings _settings;
		private readonly AppSettings.HighFrequencyTradingSettings _serviceSettings;
		private readonly IServiceCollection _services;
		private readonly ILog _log;

		public ServiceModule(AppSettings settings, ILog log)
		{
			_settings = settings;
			_serviceSettings = _settings.HighFrequencyTradingService;
			_log = log;

			_services = new ServiceCollection();
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(_serviceSettings)
				.SingleInstance();

			builder.RegisterInstance(_log)
				.As<ILog>()
				.SingleInstance();

			RegisterApiKeyService(builder);

			RegisterMatchingEngine(builder);

			RegisterBalances(builder);

			RegisterOrderBooks(builder);

			RegisterAssets(builder);

			BindRabbitMq(builder);

			builder.Populate(_services);
		}

		private static void RegisterApiKeyService(ContainerBuilder builder)
		{
			builder.RegisterType<HealthService>()
				.As<IHealthService>()
				.SingleInstance();

			builder.RegisterType<ApiKeyService>()
				.As<IApiKeyValidator>()
				.SingleInstance();

			builder.RegisterType<ApiKeyService>()
				.As<IApiKeyGenerator>()
				.SingleInstance();

			builder.RegisterType<ApiKeyService>()
				.As<IClientResolver>()
				.SingleInstance();
		}

		private static void RegisterOrderBooks(ContainerBuilder builder)
		{
			builder.RegisterType<OrderBookService>()
				.As<IOrderBooksService>()
				.SingleInstance();
		}

		private void RegisterMatchingEngine(ContainerBuilder builder)
		{
			var socketLog = new SocketLogDynamic(i => { },
				str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str));

			builder.BindMeClient(_settings.MatchingEngineClient.IpEndpoint.GetClientIpEndPoint(), socketLog);

			builder.RegisterType<MatchingEngineAdapter>()
				.As<IMatchingEngineAdapter>()
				.SingleInstance();
		}

		private void RegisterBalances(ContainerBuilder builder)
		{
			builder.RegisterInstance<IWalletsRepository>(
				AzureRepoFactories.CreateAccountsRepository(_serviceSettings.Db.BalancesInfoConnString, _log));
		}

		private void RegisterAssets(ContainerBuilder builder)
		{
			_services.UseAssetsClient(AssetServiceSettings.Create(
				new Uri(_serviceSettings.Dictionaries.AssetsServiceUrl),
				_serviceSettings.Dictionaries.CacheExpirationPeriod));

			builder.RegisterType<AssetPairsManager>()
				.As<IAssetPairsManager>()
				.WithParameter(new TypedParameter(typeof(TimeSpan), _serviceSettings.Dictionaries.CacheExpirationPeriod))
				.SingleInstance();
		}

		private void BindRabbitMq(ContainerBuilder builder)
		{
			builder.RegisterType<LimitOrdersConsumer>().SingleInstance().AutoActivate();
			builder.RegisterInstance(_serviceSettings.LimitOrdersFeed);
		}
	}
}
