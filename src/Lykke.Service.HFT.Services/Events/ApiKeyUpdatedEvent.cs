using MessagePack;

namespace Lykke.Service.HFT.Services.Events
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ApiKeyUpdatedEvent
    {
        public string ApiKey { get; set; }
        public string WalletId { get; set; }
        public bool Enabled { get; set; }
    }
}
