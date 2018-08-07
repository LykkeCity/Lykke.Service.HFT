using Autofac;
using Lykke.Service.HFT.Core;
using Lykke.SettingsReader;
using StackExchange.Redis;

namespace Lykke.Service.HFT.Services
{
    public class RedisModule : Module
    {
        private readonly CacheSettings _settings;

        public RedisModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue.HighFrequencyTradingService.Cache;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var redis = ConnectionMultiplexer.Connect(_settings.RedisConfiguration);

            builder.RegisterInstance(redis).SingleInstance();
            builder.Register(
                c =>
                    c.Resolve<ConnectionMultiplexer>()
                        .GetServer(redis.GetEndPoints()[0]));

            builder.Register(
                c =>
                    c.Resolve<ConnectionMultiplexer>()
                        .GetDatabase());
        }
    }
}
