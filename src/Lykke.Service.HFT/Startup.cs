using System;
using AspNetCoreRateLimit;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Infrastructure;
using Lykke.Service.HFT.Middleware;
using Lykke.Service.HFT.Modules;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.HFT
{
	public class Startup
	{
		public IHostingEnvironment Environment { get; }
		public IContainer ApplicationContainer { get; set; }
		public IConfigurationRoot Configuration { get; }

		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();

			Environment = env;

			Console.WriteLine($"ENV_INFO: {System.Environment.GetEnvironmentVariable("ENV_INFO")}");
		}

		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddMvc()
				.AddJsonOptions(options =>
				{
					options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
				});

			services.AddSwaggerGen(options =>
			{
				options.DefaultLykkeConfiguration("v1", "HighFrequencyTrading API");
				options.OperationFilter<ApiKeyHeaderOperationFilter>();
			});

			services.AddOptions();

			ConfigureRateLimits(services);

			var builder = new ContainerBuilder();
			var appSettings = Environment.IsDevelopment()
				? Configuration.Get<AppSettings>()
				: HttpSettingsLoader.Load<AppSettings>(Configuration.GetValue<string>("SettingsUrl"));
			var log = CreateLogWithSlack(services, appSettings);

			services.AddDistributedRedisCache(options =>
			{
				options.Configuration = appSettings.HighFrequencyTradingService.CacheSettings.RedisConfiguration;
				options.InstanceName = appSettings.HighFrequencyTradingService.CacheSettings.ApiKeyCacheInstance;
			});

			builder.RegisterModule(new ServiceModule(appSettings, log));
			builder.Populate(services);

			ApplicationContainer = builder.Build();

			return new AutofacServiceProvider(ApplicationContainer);
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseLykkeMiddleware("HighFrequencyTrading", ex => new { Message = "Technical problem" });
			app.UseMiddleware<KeyAuthMiddleware>();
			app.UseMiddleware<CustomThrottlingMiddleware>();

			// Use API Authentication
			//app.UseLykkeApiAuth(conf =>
			//    conf.ApiId = Configuration.GetValue<string>("LykkeTempApi:ApiId"));

			app.UseMvc();
			app.UseSwagger();
			app.UseSwaggerUi();

			appLifetime.ApplicationStopped.Register(() =>
			{
				ApplicationContainer.Dispose();
			});
		}


		private static ILog CreateLogWithSlack(IServiceCollection services, AppSettings settings)
		{
			var consoleLogger = new LogToConsole();
			var aggregateLogger = new AggregateLogger();

			aggregateLogger.AddLog(consoleLogger);

			// Creating slack notification service, which logs own azure queue processing messages to aggregate log
			var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
			{
				ConnectionString = settings.SlackNotifications.AzureQueue.ConnectionString,
				QueueName = settings.SlackNotifications.AzureQueue.QueueName
			}, aggregateLogger);

			var dbLogConnectionString = settings.HighFrequencyTradingService.Db.LogsConnString;

			// Creating azure storage logger, which logs own messages to concole log
			if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
			{
				const string appName = Constants.ComponentName;

				var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
					appName,
					AzureTableStorage<LogEntity>.Create(() => dbLogConnectionString, "HighFrequencyTradingLog", consoleLogger),
					consoleLogger);

				var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(appName, slackService, consoleLogger);

				var azureStorageLogger = new LykkeLogToAzureStorage(
					appName,
					persistenceManager,
					slackNotificationsManager,
					consoleLogger);

				azureStorageLogger.Start();

				aggregateLogger.AddLog(azureStorageLogger);
			}

			return aggregateLogger;
		}

		private void ConfigureRateLimits(IServiceCollection services)
		{
			services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
			services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
			services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
		}
	}
}
