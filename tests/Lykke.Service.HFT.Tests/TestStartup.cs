using JetBrains.Annotations;
using Lykke.Service.HFT.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.HFT.Tests
{
    public class TestStartup
    {
        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging()
                .AddAuthentication(HmacAuthOptions.AuthenticationScheme)
                .AddScheme<HmacAuthOptions, HmacAuthHandler>(HmacAuthOptions.AuthenticationScheme, null);
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
