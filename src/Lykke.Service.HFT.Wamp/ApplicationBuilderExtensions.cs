using System;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
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

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                var jsonSerializer = JsonSerializer.Create(jsonSettings);
                host.RegisterTransport(new AspNetCoreWebSocketTransport(builder),
                    new JTokenJsonBinding(jsonSerializer),
                    new JTokenMsgpackBinding(jsonSerializer));
            });

            host.Open();
        }
    }
}
