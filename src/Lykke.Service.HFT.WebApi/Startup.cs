using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Extensions.Configuration;
using Lykke.Service.HFT.Abstractions;
using Lykke.Service.HFT.WebApi.Infrastructure;
using Lykke.Service.HFT.WebApi.Middleware;
using Lykke.Service.HFT.WebApi.Models;
using Lykke.Service.HFT.WebApi.Modules;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Service.HFT.WebApi
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			Environment = env;

			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				// Get configurations from settings URL, for more details:
				// https://github.com/LykkeCity/DotNetCoreServiceTemplate
				//.AddFromConfiguredUrl("TEMPLATE_API_SETTINGS_URL")
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public static IHostingEnvironment Environment { get; private set; }
		public static IConfigurationRoot Configuration { get; private set; }

		public IContainer ApplicationContainer { get; private set; }

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


			var appSettings = Environment.IsDevelopment()
				? Configuration.Get<AppSettings>()
				: HttpSettingsLoader.Load<AppSettings>(Configuration.GetValue<string>("SettingsUrl"));
			//var appSettings = HttpSettingsLoader.Load<AppSettings>(Configuration.GetValue<string>("SettingsUrl"));
			//todo: JsonStringEmptyException
			var log = CreateLog(services, appSettings);

			// test init redis cache
			services.AddDistributedRedisCache(options =>
			{
				options.Configuration = appSettings.HighFrequencyTradingService.CacheSettings.RedisConfiguration;
				options.InstanceName = appSettings.HighFrequencyTradingService.CacheSettings.ApiKeyCacheInstance;
			});

			var builder = new ContainerBuilder();
			builder.RegisterModule(new ServiceModule(appSettings.HighFrequencyTradingService, log));
			builder.Populate(services);
			ApplicationContainer = builder.Build();

			return new AutofacServiceProvider(ApplicationContainer);
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env,
			ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
		{
			app.UseMiddleware<KeyAuthMiddleware>();

			loggerFactory.AddConsole();
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseLykkeMiddleware(Constants.ComponentName, ex => new ErrorResponse { ErrorMessage = "Technical problem" });

			// Use API Authentication
			//app.UseLykkeApiAuth(conf =>
			//    conf.ApiId = Configuration.GetValue<string>("LykkeTempApi:ApiId"));

			app.UseMvc();
			app.UseSwagger();
			app.UseSwaggerUi();

			// Dispose resources that have been resolved in the application container
			appLifetime.ApplicationStopped.Register(() => ApplicationContainer.Dispose());
		}


		private static ILog CreateLog(IServiceCollection services, AppSettings settings)
		{
			var logToConsole = new LogToConsole();
			var logAggregate = new LogAggregate();

			logAggregate.AddLogger(logToConsole);

			var log = logAggregate.CreateLogger();

			return log;
		}

	}
}
