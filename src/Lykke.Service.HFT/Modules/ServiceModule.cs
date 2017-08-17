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
		private readonly HighFrequencyTradingSettings _settings;
		private readonly IServiceCollection _services;
		private readonly ILog _log;

		public ServiceModule(HighFrequencyTradingSettings settings, ILog log)
		{
			_settings = settings;
			_services = new ServiceCollection();
			_log = log;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(_settings)
				.SingleInstance();

			builder.RegisterInstance(_log)
				.As<ILog>()
				.SingleInstance();

			RegisterApiKeyService(builder);

			RegisterMatchingEngine(builder);

			RegisterBalances(builder);

			RegisterOrderBooks(builder);

			RegisterAssets(builder);

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

			builder.BindMeClient(_settings.MatchingEngine.IpEndpoint.GetClientIpEndPoint(), socketLog);

			builder.RegisterType<MatchingEngineAdapter>()
				.As<IMatchingEngineAdapter>()
				.SingleInstance();
		}

		private void RegisterBalances(ContainerBuilder builder)
		{
			builder.RegisterInstance<IWalletsRepository>(
				AzureRepoFactories.CreateAccountsRepository(_settings.Db.BalancesInfoConnString, _log));
		}

		private void RegisterAssets(ContainerBuilder builder)
		{
			_services.UseAssetsClient(AssetServiceSettings.Create(
				new Uri(_settings.Dictionaries.AssetsServiceUrl),
				_settings.Dictionaries.CacheExpirationPeriod));

			builder.RegisterType<AssetPairsManager>()
				.As<IAssetPairsManager>()
				.WithParameter(new TypedParameter(typeof(TimeSpan), _settings.Dictionaries.CacheExpirationPeriod))
				.SingleInstance();
		}

	}
}
