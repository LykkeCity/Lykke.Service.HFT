using Autofac;
using Lykke.Service.HFT.Core.Settings;
using Lykke.Service.HFT.Wamp.Consumers;
using Lykke.Service.HFT.Wamp.Security;
using Lykke.SettingsReader;
using WampSharp.V2;
using WampSharp.V2.Authentication;

namespace Lykke.Service.HFT.Wamp.Modules
{
    public class WampModule : Module
    {
        private readonly HighFrequencyTradingSettings _settings;

        public WampModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue.HighFrequencyTradingService;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WampSessionAuthenticatorFactory>()
                .As<IWampSessionAuthenticatorFactory>()
                .SingleInstance();

            builder.RegisterType<WampAuthenticationHost>()
                .As<IWampHost>()
                .SingleInstance();

            builder.Register(x => x.Resolve<IWampHost>().RealmContainer.GetRealmByName(Constans.RealmName))
                .SingleInstance();

            BindRabbitMq(builder);
        }

        private void BindRabbitMq(ContainerBuilder builder)
        {
            builder.RegisterType<LimitOrdersConsumer>()
                .WithParameter(TypedParameter.From(_settings.LimitOrdersFeed))
                .SingleInstance().AutoActivate();
        }
    }
}