using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Sdk.Health;
using Lykke.Sdk.Middleware;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Core.Settings;
using Lykke.Service.HFT.Infrastructure;
using Lykke.Service.HFT.Middleware;
using Lykke.Service.HFT.Services;
using Lykke.Service.HFT.Swagger;
using Lykke.Service.HFT.Wamp;
using Lykke.Service.HFT.Wamp.Modules;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
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
#if DEBUG
            TelemetryConfiguration.Active.DisableTelemetry = true;
#endif

            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Logs = logs =>
                {
                    logs.AzureTableName = "HighFrequencyTradingLog";
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.HighFrequencyTradingService.Db.LogsConnString;
                };

                options.SwaggerOptions = _swaggerOptions;

                options.Swagger = swagger =>
                {
                    swagger.OperationFilter<ApiKeyHeaderOperationFilter>();
                    swagger.DescribeAllEnumsAsStrings();

                    // Include XML comments from contracts.
                    swagger.IncludeXmlComments(Path.Combine(Environment.ContentRootPath, typeof(ResponseModel).Assembly.GetName().Name + ".xml"));

                    swagger.DocumentFilter<DefaultFilter>();
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
                    };
                });

            app.UseWampHost(provider.GetService<IWampHost>());
        }
    }
}
