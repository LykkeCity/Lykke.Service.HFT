using System;
using Autofac;
using Common.Log;
using Lykke.Service.HFT.Client.AutorestClient;

namespace Lykke.Service.HFT.Client
{
    public static class AutofacExtension
    {
        public static void RegisterHftApiClient(this ContainerBuilder builder, string serviceUrl, ILog log)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterInstance(new HighFrequencytradingAPI(new Uri(serviceUrl))).As<IHighFrequencytradingAPI>().SingleInstance();
        }
    }
}
