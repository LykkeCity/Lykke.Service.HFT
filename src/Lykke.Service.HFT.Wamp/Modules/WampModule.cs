﻿using Autofac;
using Autofac.Core;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Lykke.Service.HFT.Wamp.Auth;
using Lykke.Service.HFT.Wamp.Consumers;
using WampSharp.V2;
using WampSharp.V2.Authentication;

namespace Lykke.Service.HFT.Wamp.Modules
{
    public class WampModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WampSessionAuthenticatorFactory>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IApiKeyValidator),
                        (pi, ctx) => ctx.Resolve<IApiKeyValidator>()))
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IClientResolver),
                        (pi, ctx) => ctx.Resolve<IClientResolver>()))
                .As<IWampSessionAuthenticatorFactory>()
                .SingleInstance();

            builder.RegisterType<WampAuthenticationHost>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IWampSessionAuthenticatorFactory),
                        (pi, ctx) => ctx.Resolve<IWampSessionAuthenticatorFactory>()))
                .As<IWampHost>()
                .SingleInstance();

            builder.Register(x => x.Resolve<IWampHost>().RealmContainer.GetRealmByName(Constans.RealmName))
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