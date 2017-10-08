using Autofac;
using Lykke.Service.HFT.Core;
using StackExchange.Redis;

namespace Lykke.Service.HFT.Services
{
	public class RedisModule : Module
	{
		private readonly CacheSettings _settings;

		public RedisModule(CacheSettings settings)
		{
			_settings = settings;
		}

		protected override void Load(ContainerBuilder builder)
		{
			BindRedis(builder);
		}

		private void BindRedis(ContainerBuilder builder)
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
