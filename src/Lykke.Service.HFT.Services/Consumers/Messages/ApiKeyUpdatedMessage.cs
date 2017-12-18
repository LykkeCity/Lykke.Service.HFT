using System;

namespace Lykke.Service.HFT.Services.Consumers.Messages
{
    public class ApiKeyUpdatedMessage
    {
        public ApiKey ApiKey { get; set; }
    }

    public class ApiKey
    {
        public Guid Id { get; set; }
        public string WalletId { get; set; }
        public bool Enabled { get; set; }
    }
}
