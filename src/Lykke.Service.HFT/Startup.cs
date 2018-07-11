using System;
using System.IO;
using System.Linq;
using AspNetCoreRateLimit;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Sdk.Health;
using Lykke.Sdk.Middleware;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Infrastructure;
using Lykke.Service.HFT.Middleware;
using Lykke.Service.HFT.Services;
using Lykke.Service.HFT.Wamp;
using Lykke.Service.HFT.Wamp.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WampSharp.V2;

namespace Lykke.Service.HFT
{
    internal sealed class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiVersion = "v1",
            ApiTitle = "High-frequency trading API"
        };

        public IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment env)
        {
            Environment = env;
        }

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Logs = logs =>
                {
                    logs.AzureTableName = "HighFrequencyTradingLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.HighFrequencyTradingService.Db.LogsConnString;
                };

                options.SwaggerOptions = _swaggerOptions;

                options.Swagger = swagger =>
                {
                    swagger.OperationFilter<ApiKeyHeaderOperationFilter>();
                    swagger.DescribeAllEnumsAsStrings();

                    // Include XML comments from contracts.
                    swagger.IncludeXmlComments(Path.Combine(Environment.ContentRootPath, typeof(ResponseModel).Assembly.GetName().Name + ".xml"));
                };

                options.RegisterAdditionalModules = x =>
                {
                    x.RegisterModule<WampModule>();
                    x.RegisterModule<RedisModule>();
                };

                options.Extend = (sc, settings) =>
                {
                    sc
                        .AddOptions()
                        .AddAuthentication(KeyAuthOptions.AuthenticationScheme)
                        .AddScheme<KeyAuthOptions, KeyAuthHandler>(KeyAuthOptions.AuthenticationScheme, null);

                    ConfigureRateLimits(services, settings.CurrentValue.HighFrequencyTradingService.IpRateLimiting);
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            var provider = app.ApplicationServices;

            app.UseLykkeConfiguration(options =>
                {
                    options.SwaggerOptions = _swaggerOptions;

                    options.WithMiddleware = x =>
                    {
                        x.UseMaintenanceMode<AppSettings>(settings => new MaintenanceMode
                        {
                            Enabled = settings.HighFrequencyTradingService.MaintenanceMode?.Enabled ?? false,
                            Reason = settings.HighFrequencyTradingService.MaintenanceMode?.Reason
                        });
                        x.UseAuthentication();
                        x.UseIpRateLimiting();
                        x.UseClientRateLimiting();
                    };
                });

            app.UseWampHost(provider.GetService<IWampHost>());
        }

        private static void ConfigureRateLimits(IServiceCollection services, RateLimitSettings.RateLimitCoreOptions rateLimitOptions)
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
