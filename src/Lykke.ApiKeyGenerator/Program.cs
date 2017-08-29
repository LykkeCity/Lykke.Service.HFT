using System;
using Autofac;
using Lykke.Service.HFT.Core;
using Lykke.SettingsReader;

namespace Lykke.ApiKeyGenerator
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: <AppSettings url> <client id>");
                return -1;
            }

            var appSettings = HttpSettingsLoader.Load<AppSettings>(args[0]);

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ServiceModule(appSettings));
            var applicationContainer = builder.Build();

            var apiKey = applicationContainer.Resolve<IApiKeyGenerator>().GenerateApiKeyAsync(args[1]).Result;
            Console.WriteLine(apiKey);
            return 0;
        }
    }
}
