using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core.Settings
{
    public class DbSettings
    {
        [AzureBlobCheck]
        public string LogsConnString { get; set; }

        [MongoCheck]
        public string OrderStateConnString { get; set; }

        [AzureTableCheck]
        public string OrdersArchiveConnString { get; set; }
    }
}