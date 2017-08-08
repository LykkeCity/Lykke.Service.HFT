using System;
using Lykke.Http;
using Lykke.Service.HFT.Abstractions;
using Lykke.Service.HFT.WebClient;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHFTWebClient(this IServiceCollection services,
            string apiUrl, string apiKey)
        {
            return AddHFTWebClient(services, config =>
            {
                config.ApiUrl = apiUrl;
                config.ApiKey = apiKey;
            });
        }

        public static IServiceCollection AddHFTWebClient(this IServiceCollection services,
            Action<HFTRestClientConfig> configurator)
        {
            var config = new HFTRestClientConfig();
            configurator.Invoke(config);

            return AddHFTWebClient(services, config);
        }

        public static IServiceCollection AddHFTWebClient(this IServiceCollection services,
            HFTRestClientConfig config)
        {
            services.AddSingleton(config);
            services.AddSingleton<ISamplesRepository, SamplesRepositoryClient>();

            return services;
        }
    }
}

namespace Lykke.Service.HFT.WebClient
{
    public class HFTRestClientConfig : RestClientConfig
    {

    }
}