using System.Net;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core.Settings
{
    public class MatchingEngineSettings
    {
        public IpEndpointSettings IpEndpoint { get; set; }

        public class IpEndpointSettings
        {
            [TcpCheck("Port")]
            public string Host { get; set; }
            public int Port { get; set; }

            public IPEndPoint GetClientIpEndPoint()
            {
                if (IPAddress.TryParse(Host, out var ipAddress))
                    return new IPEndPoint(ipAddress, Port);

                var addresses = Dns.GetHostAddressesAsync(Host).Result;
                return new IPEndPoint(addresses[0], Port);
            }
        }

    }
}