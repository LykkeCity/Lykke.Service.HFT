using System;
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
using Lykke.Logs.Slack;
using Lykke.MonitoringServiceApiCaller;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Infrastructure;
using Lykke.Service.HFT.Middleware;
using Lykke.Service.HFT.Modules;
using Lykke.Service.HFT.Services;
using Lykke.Service.HFT.Wamp;
using Lykke.Service.HFT.Wamp.Modules;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WampSharp.V2;
using WampSharp.V2.MetaApi;
using WampSharp.V2.Realm;

namespace Lykke.Service.HFT
{
    public class Startup
    {
        private string _monitoringServiceUrl;
        private const string ApiVersion = "v1";
        private const string ApiTitle = "High-frequency trading API";

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
                    options.DefaultLykkeConfiguration(ApiVersion, ApiTitle);
                    options.OperationFilter<ApiKeyHeaderOperationFilter>();
                    options.DescribeAllEnumsAsStrings();
                });

                services.AddOptions();

                var builder = new ContainerBuilder();
                var appSettings = Configuration.LoadSettings<AppSettings>();
                _monitoringServiceUrl = appSettings.CurrentValue.MonitoringServiceClient.MonitoringServiceUrl;
                Log = CreateLogWithSlack(services, appSettings);

                ConfigureRateLimits(services, appSettings.CurrentValue.HighFrequencyTradingService.IpRateLimiting);
                builder.Populate(services);

                builder.RegisterModule(new ServiceModule(appSettings, Log));
                builder.RegisterModule(new MatchingEngineModule(appSettings, Log));
                builder.RegisterModule(new MongoDbModule(appSettings.Nested(x => x.HighFrequencyTradingService.MongoSettings)));
                builder.RegisterModule(new RedisModule(appSettings.CurrentValue.HighFrequencyTradingService.CacheSettings));
                builder.RegisterModule(new ClientsModule(appSettings, Log));
                builder.RegisterModule(new WampModule(appSettings.CurrentValue.HighFrequencyTradingService));
                builder.RegisterModule(new CqrsModule(appSettings.Nested(x => x.HighFrequencyTradingService), Log));

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
                var mode = ApplicationContainer.ResolveOptional<AppSettings.MaintenanceMode>();
                if (mode != null && mode.Enabled)
                {
                    app.Use(async (context, next) =>
                    {
                        context.Response.StatusCode = 503;
                        await context.Response.WriteAsync(mode.Reason ?? "Service on maintenance");
                    });
                }

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseLykkeMiddleware(Constants.ComponentName, ex => new { Message = "Technical problem" });

                app.UseIpRateLimiting();
                app.UseClientRateLimiting();

                app.UseAuthentication();
                app.UseMvc();
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", ApiVersion);
                });
                app.UseStaticFiles();

                var host = ApplicationContainer.Resolve<IWampHost>();
                app.UseWampHost(host);

                appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
                appLifetime.ApplicationStopped.Register(() => CleanUp().Wait());
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(Configure), "", ex).Wait();
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                await ApplicationContainer.Resolve<IStartupManager>().StartAsync();
                await AutoRegistrationInMonitoring.RegisterAsync(Configuration, _monitoringServiceUrl, Log);

                await Log.WriteMonitorAsync("", $"Env: {Program.EnvInfo}", "Started");
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
                // NOTE: Service can't receive and process requests here, so you can destroy all resources

                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", $"Env: {Program.EnvInfo}", "Terminating");
                }

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();

                    ApplicationContainer.Resolve<IWampHostedRealm>()?.HostMetaApiService()?.Dispose();
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

            // Creating azure storage logger, which logs own messages to console log
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

                //var logToSlack = LykkeLogToSlack.Create(slackService, "hft-api", LogLevel.Error | LogLevel.FatalError | LogLevel.Warning);
                var logToSlack = LykkeLogToSlack.Create(slackService, "hft-api", LogLevel.All);
                aggregateLogger.AddLog(logToSlack);
            }

            return aggregateLogger;
        }

        private void ConfigureRateLimits(IServiceCollection services, RateLimitSettings.RateLimitCoreOptions rateLimitOptions)
        {
            services.Configure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = rateLimitOptions.EnableEndpointRateLimiting;
                options.StackBlockedRequests = rateLimitOptions.StackBlockedRequests;
                options.GeneralRules = rateLimitOptions.GeneralRules.Select(x => new RateLimitRule
                {
                    Endpoint = x.Endpoint,
                    Limit = x.Limit,
                    Period = x.Period
                }).ToList();
            });
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

            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        }
    }
}
