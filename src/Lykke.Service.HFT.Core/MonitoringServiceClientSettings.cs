using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core
{
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string MonitoringServiceUrl { get; set; }
    }
}
