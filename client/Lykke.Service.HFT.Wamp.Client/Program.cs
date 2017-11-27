using System;
using System.Threading;
using Common;
using WampSharp.V2;
using WampSharp.V2.Client;
using Konscious.Security.Cryptography;
using Lykke.Service.HFT.Contracts.Events;

namespace Lykke.Service.HFT.Wamp.Client
{
    public class Program
    {
        public static string EnvInfo => Environment.GetEnvironmentVariable("ENV_INFO");
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Wrong number of arguments. Expected: client id.");
                return -1;
            }

            var clientId = args[0];

            var factory = new DefaultWampChannelFactory();
            var serverAddress = "ws://localhost:5000/ws";
            var realm = "HftApi";
            var channel = factory.CreateJsonChannel(serverAddress, realm);

            while (!channel.RealmProxy.Monitor.IsConnected)
            {
                try
                {
                    Console.WriteLine($"Trying to connect to server {serverAddress}...");
                    channel.Open().Wait();
                }
                catch
                {
                    Console.WriteLine("Retrying in 5 sec...");
                    Thread.Sleep(5000);
                }
            }
            Console.WriteLine($"Connected to server {channel}");
            using (Subscribe(channel.RealmProxy, clientId))
            {
                Console.ReadKey();
            }
            Console.ReadKey();
            channel.Close();
            Console.WriteLine("Terminated");
            return 0;
        }

        private static IDisposable Subscribe(IWampRealmProxy proxy, string clientId)
        {
            var hashAlgorithm = new HMACBlake2B(128);
            hashAlgorithm.Initialize();

            var subscriptionId = hashAlgorithm.ComputeHash(clientId.ToUtf8Bytes()).ToBase64();
            var subscription = proxy.Services.GetSubject<LimitOrderUpdateEvent>($"orders.limit.{subscriptionId}")
                .Subscribe(info =>
                {
                    Console.WriteLine($"Got event: {info.ToJson()}");
                });
            Console.WriteLine($"Subscribed to events. ClientId: {clientId}; subscription id: {subscriptionId}");
            return subscription;
        }
    }
}
