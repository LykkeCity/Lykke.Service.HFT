using Autofac;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Wamp.Consumers;
using Lykke.Service.HFT.Wamp.Security;
using WampSharp.V2;
using WampSharp.V2.Authentication;

namespace Lykke.Service.HFT.Wamp.Modules
{
    public class WampModule : Module
    {
        private readonly AppSettings.HighFrequencyTradingSettings _settings;

        public WampModule(AppSettings.HighFrequencyTradingSettings settings)
        {
            _settings = settings;
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