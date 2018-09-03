using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        public string ExchangeName { get; set; }
    }
}