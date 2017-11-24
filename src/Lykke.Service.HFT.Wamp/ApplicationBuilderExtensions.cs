using System;
using Microsoft.AspNetCore.Builder;
using WampSharp.AspNetCore.WebSockets.Server;
using WampSharp.Binding;
using WampSharp.V2;

namespace Lykke.Service.HFT.Wamp
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseWampHost(this IApplicationBuilder app, IWampHost host)
        {
            app.Map("/ws", builder =>
            {
                builder.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromMinutes(1) });

                host.RegisterTransport(new AspNetCoreWebSocketTransport(builder),
                    new JTokenJsonBinding(),
                    new JTokenMsgpackBinding());
            });

            host.Open();
        }
    }
}
