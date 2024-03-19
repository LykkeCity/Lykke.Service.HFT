using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.HFT.Services.Consumers;
using Microsoft.Extensions.DependencyInjection;
using WampSharp.V2.MetaApi;
using WampSharp.V2.Realm;

namespace Lykke.Service.HFT.Services
{
    [UsedImplicitly]
    internal class ShutdownManager : IShutdownManager
    {
        private readonly IServiceProvider _provider;
        private readonly ClientSettingsUpdatesConsumer _clientSettingsUpdatesConsumer;

        public ShutdownManager(IServiceProvider provider,
            ClientSettingsUpdatesConsumer clientSettingsUpdatesConsumer)
        {
            _provider = provider;
            _clientSettingsUpdatesConsumer = clientSettingsUpdatesConsumer;
        }

        public Task StopAsync()
        {
            _clientSettingsUpdatesConsumer.Stop();

            var realm = _provider.GetService<IWampHostedRealm>();
            realm?.HostMetaApiService()?.Dispose();

            return Task.CompletedTask;
        }
    }
}