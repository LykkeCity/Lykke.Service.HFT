﻿using System;

namespace Lykke.Service.HFT.Core.Domain
{
    public class ApiKey : IHasId
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public string ClientId { get; set; }
        public string WalletId { get; set; }
        public DateTime? ValidTill { get; set; }
        public bool Apiv2Only { get; set; }
    }
}
