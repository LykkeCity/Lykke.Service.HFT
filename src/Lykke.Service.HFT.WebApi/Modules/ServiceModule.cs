using System;
using Autofac;
using Common;
using Common.Log;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.HFT.Abstractions;
using Lykke.Service.HFT.Abstractions.Services;
using Lykke.Service.HFT.Services;

namespace Lykke.Service.HFT.WebApi.Modules
{
	public class ServiceModule : Module
	{
		private readonly HighFrequencyTradingSettings _settings;
		private readonly ILog _log;

		public ServiceModule(HighFrequencyTradingSettings settings, ILog log)
		{
			_settings = settings;
			_log = log;
		}

		protected override void Load(ContainerBuilder builder)
		{
			var socketLog = new SocketLogDynamic(i => { },
				str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str));

			builder.BindMeClient(_settings.MatchingEngine.IpEndpoint.GetClientIpEndPoint(), socketLog);

			builder.RegisterType<ApiKeyService>()
				.As<IApiKeyValidator>()
				.SingleInstance();

			builder.RegisterType<ApiKeyService>()
				.As<IApiKeyGenerator>()
				.SingleInstance();

			builder.RegisterType<ApiKeyService>()
				.As<IClientResolver>()
				.SingleInstance();

			builder.RegisterInstance(_settings)
				.SingleInstance();

			builder.RegisterInstance(_log)
				.As<ILog>()
				.SingleInstance();

			builder.RegisterType<MatchingEngineAdapter>()
				.As<IMatchingEngineAdapter>()
				.SingleInstance();
		}
	}
}
