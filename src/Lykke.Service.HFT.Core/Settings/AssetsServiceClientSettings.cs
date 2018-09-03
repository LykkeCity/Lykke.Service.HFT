using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core.Settings
{
    public class AssetsServiceClientSettings
    {
        [HttpCheck("api/IsAlive")]
        public string ServiceUrl { get; set; }
    }
}