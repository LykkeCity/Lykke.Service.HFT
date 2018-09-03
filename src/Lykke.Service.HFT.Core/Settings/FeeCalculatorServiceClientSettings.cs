using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core.Settings
{
    public class FeeCalculatorServiceClientSettings
    {
        [HttpCheck("api/IsAlive")]
        public string ServiceUrl { get; set; }
    }
}