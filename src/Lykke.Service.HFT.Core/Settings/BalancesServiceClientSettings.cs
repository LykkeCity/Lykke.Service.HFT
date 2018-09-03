using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core.Settings
{
    public class BalancesServiceClientSettings
    {
        [HttpCheck("api/IsAlive")]
        public string ServiceUrl { get; set; }
    }
}