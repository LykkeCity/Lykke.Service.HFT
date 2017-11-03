﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Lykke.Service.HFT.Infrastructure;
using Lykke.Service.HFT.Middleware;
using Lykke.Service.HFT.Modules;
using Lykke.Service.HFT.Services;
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
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public ILog Log { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                        .AddJsonOptions(options =>
                        {
                            options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                            options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                        });
                services.AddAuthentication(KeyAuthOptions.AuthenticationScheme)
                    .AddScheme<KeyAuthOptions, KeyAuthHandler>(KeyAuthOptions.AuthenticationScheme, "", options => { });

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", "HighFrequencyTrading API");
                    options.OperationFilter<ApiKeyHeaderOperationFilter>();
                    options.DescribeAllEnumsAsStrings();
                });

                services.AddOptions();

                var builder = new ContainerBuilder();
                var appSettings = Configuration.LoadSettings<AppSettings>();
                Log = CreateLogWithSlack(services, appSettings);

                ConfigureRateLimits(services, appSettings.CurrentValue.HighFrequencyTradingService.IpRateLimiting);

                builder.RegisterModule(new ServiceModule(appSettings, Log));
                builder.RegisterModule(new MatchingEngineModule(appSettings.Nested(x => x.MatchingEngineClient),
                    appSettings.Nested(x => x.HighFrequencyTradingService)));
                builder.RegisterModule(new MongoDbModule(appSettings.Nested(x => x.HighFrequencyTradingService.MongoSettings)));
                builder.RegisterModule(new RedisModule(appSettings.CurrentValue.HighFrequencyTradingService.CacheSettings));
                builder.Populate(services);

                ApplicationContainer = builder.Build();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseLykkeMiddleware("HighFrequencyTrading", ex => new { Message = "Technical problem" });
                app.UseClientRateLimiting();

                app.UseAuthentication();
                app.UseMvc();
                app.UseSwagger();
                app.UseSwaggerUi();
                app.UseStaticFiles();

                appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
                appLifetime.ApplicationStopped.Register(() => CleanUp().Wait());
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet recieve and process requests here

                await ApplicationContainer.Resolve<IApiKeyCacheInitializer>().InitApiKeyCache();

                await Log.WriteMonitorAsync("", "", "Started");
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
                throw;
            }
        }


        private async Task CleanUp()
        {
            try
            {
                // NOTE: Service can't recieve and process requests here, so you can destroy all resources

                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", "", "Terminating");
                }

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();
                }
                throw;
            }
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.HighFrequencyTradingService.Db.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            // Creating azure storage logger, which logs own messages to concole log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "HighFrequencyTradingLog", consoleLogger),
                    consoleLogger);

                var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

                var azureStorageLogger = new LykkeLogToAzureStorage(
                    persistenceManager,
                    slackNotificationsManager,
                    consoleLogger);

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }

            return aggregateLogger;
        }

        private void ConfigureRateLimits(IServiceCollection services, RateLimitSettings.RateLimitCoreOptions rateLimitOptions)
        {
            services.Configure<ClientRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = rateLimitOptions.EnableEndpointRateLimiting;
                options.ClientIdHeader = KeyAuthOptions.DefaultHeaderName;
                options.StackBlockedRequests = rateLimitOptions.StackBlockedRequests;
                options.GeneralRules = rateLimitOptions.GeneralRules.Select(x => new RateLimitRule
                {
                    Endpoint = x.Endpoint,
                    Limit = x.Limit,
                    Period = x.Period
                }).ToList();
            });

            services.AddSingleton<IClientPolicyStore, DistributedCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
        }
    }
}
