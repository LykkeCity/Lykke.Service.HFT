using MessagePack;

namespace Lykke.Service.HFT.Services.Events
{
    // Owned by the Lykke.Service.HftInternalService
    [MessagePackObject(keyAsPropertyName: true)]
    public class ApiKeyUpdatedEvent
    {
        public string ApiKey { get; set; }
        public string WalletId { get; set; }
        public bool Enabled { get; set; }
        public bool Apiv2Only { get; set; }
        public string ClientId { get; set; }
    }
}
