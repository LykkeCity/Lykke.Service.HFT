using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.MonitoringServiceApiCaller;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Infrastructure;
using Lykke.Service.HFT.Middleware;
using Lykke.Service.HFT.Modules;
using Lykke.Service.HFT.Services;
using Lykke.Service.HFT.Wamp;
using Lykke.Service.HFT.Wamp.Modules;
using Lykke.SettingsReader;
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

        private ILog _log;
        private IHealthNotifier _healthNotifier;

        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }

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

                    // Include XML comments from contracts.
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    options.IncludeXmlComments(Path.Combine(baseDirectory, typeof(ResponseModel).Assembly.GetName().Name + ".xml"));
                });

                services.AddOptions();

                var builder = new ContainerBuilder();
                var appSettings = Configuration.LoadSettings<AppSettings>();
                _monitoringServiceUrl = appSettings.CurrentValue.MonitoringServiceClient.MonitoringServiceUrl;

                services.AddLykkeLogging(
                    appSettings.Nested(x => x.HighFrequencyTradingService.Db.LogsConnString),
                    "HighFrequencyTradingLog",
                    appSettings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                    appSettings.CurrentValue.SlackNotifications.AzureQueue.QueueName
                );
                
                ConfigureRateLimits(services, appSettings.CurrentValue.HighFrequencyTradingService.IpRateLimiting);
                builder.Populate(services);

                builder.RegisterModule(new ServiceModule(appSettings));
                builder.RegisterModule(new MatchingEngineModule(appSettings));
                builder.RegisterModule(new MongoDbModule(appSettings.Nested(x => x.HighFrequencyTradingService.MongoSettings)));
                builder.RegisterModule(new RedisModule(appSettings.CurrentValue.HighFrequencyTradingService.CacheSettings));
                builder.RegisterModule(new ClientsModule(appSettings));
                builder.RegisterModule(new WampModule(appSettings.CurrentValue.HighFrequencyTradingService));
                builder.RegisterModule(new CqrsModule(appSettings.Nested(x => x.HighFrequencyTradingService)));

                ApplicationContainer = builder.Build();
                _log = ApplicationContainer.Resolve<ILogFactory>().CreateLog(this);
                _healthNotifier = ApplicationContainer.Resolve<IHealthNotifier>();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                _log?.Critical(ex, process: nameof(ConfigureServices));
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

                app.UseLykkeMiddleware(ex => new { Message = "Technical problem" });

                app.UseIpRateLimiting();
                app.UseClientRateLimiting();

                app.UseAuthentication();
                app.UseMvc();
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", ApiVersion);
                    x.DocumentTitle = ApiTitle;
                });
                app.UseStaticFiles();

                var host = ApplicationContainer.Resolve<IWampHost>();
                app.UseWampHost(host);

                appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                _log?.Critical(ex, process: nameof(Configure));
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                await ApplicationContainer.Resolve<IStartupManager>().StartAsync();
                await Configuration.RegisterInMonitoringServiceAsync(_monitoringServiceUrl, _healthNotifier);

                _healthNotifier.Notify("Started", $"Env: {Program.EnvInfo}");
            }
            catch (Exception ex)
            {
                _log?.Critical(ex, process: nameof(StartApplication));
                throw;
            }
        }


        private void CleanUp()
        {
            try
            {
                _healthNotifier?.Notify("Terminating", $"Env: {Program.EnvInfo}");
                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                _log?.Critical(ex, process: nameof(CleanUp));
                ApplicationContainer.Resolve<IWampHostedRealm>()?.HostMetaApiService()?.Dispose();

                throw;
            }
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
