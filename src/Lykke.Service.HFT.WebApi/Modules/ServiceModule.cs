using System;
using Autofac;
using Common;
using Common.Log;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.HFT.Abstractions;
using Lykke.Service.HFT.Abstractions.Services;
using Lykke.Service.HFT.Services;
using Lykke.Service.HFT.WebApi.Middleware.Validator;
using Lykke.Service.HFT.WebApi.Validator.Middleware;

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
			builder.RegisterType<FixedApiKeyValidator>()
				.As<IApiKeyValidator>()
				.SingleInstance();

			builder.RegisterInstance(_settings)
				.SingleInstance();

			builder.RegisterInstance(_log)
				.As<ILog>()
				.SingleInstance();
			
			builder.RegisterType<HighFrequencyTradingService>()
				.As<IHighFrequencyTradingService>()
				.SingleInstance();


			var socketLog = new SocketLogDynamic(i => { },
				str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str));

			builder.BindMeClient(_settings.MatchingEngine.IpEndpoint.GetClientIpEndPoint(), socketLog);

		}
	}
}
