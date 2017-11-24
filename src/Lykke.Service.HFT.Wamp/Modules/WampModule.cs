using Autofac;
using WampSharp.V2;
using WampSharp.V2.Realm;

namespace Lykke.Service.HFT.Wamp.Modules
{
    public class WampModule : Module
    {
        public WampModule()
        {
        }

        protected override void Load(ContainerBuilder builder)
        {
            var host = new WampAuthenticationHost(new WampSessionAuthenticatorFactory());
            var realm = host.RealmContainer.GetRealmByName(Constans.RealmName);

            builder.RegisterInstance(host)
                .As<IWampHost>()
                .SingleInstance();

            builder.RegisterInstance(realm)
                .As<IWampHostedRealm>()
                .SingleInstance();

            BindRabbitMq(builder);
        }

        private void BindRabbitMq(ContainerBuilder builder)
        {
            builder.RegisterType<LimitOrdersConsumer>()
                .SingleInstance().AutoActivate();
        }
    }
}