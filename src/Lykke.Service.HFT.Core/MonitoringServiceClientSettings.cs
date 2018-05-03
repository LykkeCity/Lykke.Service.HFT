using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core
{
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive", false)]
        public string MonitoringServiceUrl { get; set; }
    }
}
