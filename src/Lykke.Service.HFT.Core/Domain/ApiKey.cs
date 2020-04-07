using System;

namespace Lykke.Service.HFT.Core.Domain
{
    public class ApiKey : IHasId
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public string WalletId { get; set; }
        public string SecretKey { get; set; }
        public DateTime? ValidTill { get; set; }
    }
}
