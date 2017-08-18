using System;
using System.Collections.Generic;
using AspNetCoreRateLimit;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Infrastructure;
using Lykke.Service.HFT.Middleware;
using Lykke.Service.HFT.Modules;
using Lykke.SettingsReader;
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
			var log = CreateLog();

			services.AddDistributedRedisCache(options =>
			{
				options.Configuration = appSettings.HighFrequencyTradingService.CacheSettings.RedisConfiguration;
				options.InstanceName = appSettings.HighFrequencyTradingService.CacheSettings.ApiKeyCacheInstance;
			});

			builder.RegisterModule(new ServiceModule(appSettings.HighFrequencyTradingService, log));
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


		private static ILog CreateLog()
		{
			var logToConsole = new LogToConsole();
			var logAggregate = new LogAggregate();

			logAggregate.AddLogger(logToConsole);

			var log = logAggregate.CreateLogger();

			return log;
		}

		private static void ConfigureRateLimits(IServiceCollection services)
		{
			services.Configure<IpRateLimitOptions>(options =>
			{
				options.EnableEndpointRateLimiting = true;
				options.ClientIdHeader = KeyAuthOptions.DefaultHeaderName;
				options.StackBlockedRequests = false;
				options.RealIpHeader = "X-Real-IP";
				options.GeneralRules = new List<RateLimitRule>
				{
					new RateLimitRule
					{
						Endpoint = "*",
						Limit = 300,
						Period = "1m"
					},
					new RateLimitRule
					{
						Endpoint = "get:/api/AssetPairs",
						Limit = 5,
						Period = "10m"
					},
					new RateLimitRule
					{
						Endpoint = "get:/api/Wallets",
						Limit = 60,
						Period = "1m"
					}
				};
			});

			services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
			services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
		}
	}
}
