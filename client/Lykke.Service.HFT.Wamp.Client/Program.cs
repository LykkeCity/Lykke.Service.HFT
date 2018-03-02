using System;
using System.Threading;
using Common;
using WampSharp.V2;
using WampSharp.V2.Client;

namespace Lykke.Service.HFT.Wamp.Client
{
    public class Program
    {
        private static string _serverAddress = "ws://localhost:5000/ws";
        private static string _realm = "HftApi";
        private static readonly TimeSpan RetryTimeout = TimeSpan.FromSeconds(5);
        private static string _topicUri = "orders.limit";

        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Wrong number of arguments. Expected: <client id> <server address> <realm> <topic>.");
                return -1;
            }

            var apiKey = args[0];
            if (args.Length > 1)
                _serverAddress = args[1];
            if (args.Length > 2)
                _realm = args[2];
            if (args.Length > 3)
                _topicUri = args[3];

            var factory = new DefaultWampChannelFactory();
            var authenticator = new TicketAuthenticator(apiKey);
            var channel = factory.CreateJsonChannel(_serverAddress, _realm, authenticator);
            channel.RealmProxy.Monitor.ConnectionBroken += Monitor_ConnectionBroken;
            channel.RealmProxy.Monitor.ConnectionError += Monitor_ConnectionError;
            channel.RealmProxy.Monitor.ConnectionEstablished += Monitor_ConnectionEstablished;

            while (!channel.RealmProxy.Monitor.IsConnected)
            {
                try
                {
                    Console.WriteLine($"Trying to connect to server {_serverAddress}...");
                    channel.Open().Wait();
                }
                catch
                {
                    Console.WriteLine($"Retrying in {RetryTimeout}...");
                    Thread.Sleep(RetryTimeout);
                }
            }
            Console.WriteLine($"Connected to server {channel}");
            using (Subscribe(channel.RealmProxy))
            {
                Console.WriteLine($"Subscribed to events. ClientId: {apiKey}");
                Console.ReadKey();
            }
            Console.ReadKey();
            channel.Close();
            Console.WriteLine("Terminated");
            return 0;
        }

        private static void Monitor_ConnectionEstablished(object sender, WampSharp.V2.Realm.WampSessionCreatedEventArgs e)
        {
        }

        private static void Monitor_ConnectionError(object sender, WampSharp.Core.Listener.WampConnectionErrorEventArgs e)
        {
        }

        private static void Monitor_ConnectionBroken(object sender, WampSharp.V2.Realm.WampSessionCloseEventArgs e)
        {
        }

        private static IDisposable Subscribe(IWampRealmProxy proxy)
        {
            var subject = proxy.Services.GetSubject(_topicUri);
            var subscription = subject
                .Subscribe(info =>
                {
                    Console.WriteLine($"Got event: {info.ToJson()}");
                });
            return subscription;
        }
    }
}
