using System;

namespace Lykke.Service.HFT.Core.Domain
{
    public class ApiKey : IHasId
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public DateTime? ValidTill { get; set; }
    }
}
