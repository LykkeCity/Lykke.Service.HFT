using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Sdk;
using Microsoft.Extensions.DependencyInjection;
using WampSharp.V2.MetaApi;
using WampSharp.V2.Realm;

namespace Lykke.Service.HFT.Services
{
    [UsedImplicitly]
    internal class ShutdownManager : IShutdownManager
    {
        private readonly IServiceProvider _provider;

        public ShutdownManager(IServiceProvider provider)
        {
            _provider = provider;
        }

        public Task StopAsync()
        {
            var realm = _provider.GetService<IWampHostedRealm>();
            realm?.HostMetaApiService()?.Dispose();

            return Task.CompletedTask;
        }
    }
}