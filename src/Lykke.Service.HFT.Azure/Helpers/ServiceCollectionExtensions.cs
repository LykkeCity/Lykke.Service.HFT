using System;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.HFT.Abstractions;
using Lykke.Service.HFT.Azure;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHFTAzureRepositories(this IServiceCollection services,
            Action<HFTAzureConfig> configurator)
        {
            var config = new HFTAzureConfig();
            configurator.Invoke(config);

            return AddHFTAzureRepositories(services, config);
        }

        public static IServiceCollection AddHFTAzureRepositories(this IServiceCollection services,
            HFTAzureConfig config)
        {
            services.AddSingleton<ISamplesRepository>(
                new SamplesRepository(new AzureTableStorage<SampleEntity>(
                    config.ConnectionString, "Samples", config.Logger)));

            return services;
        }
    }

    public class HFTAzureConfig
    {
        public string ConnectionString { get; set; }
        public ILog Logger { get; set; }
    }
}