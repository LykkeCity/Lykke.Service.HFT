using System.Collections.Generic;

namespace Lykke.Service.HFT.Core
{
    public class RateLimitSettings
    {
        public class RateLimitCoreOptions
        {
            public List<RateLimitRule> GeneralRules { get; set; }
            public bool StackBlockedRequests { get; set; }
            public bool EnableEndpointRateLimiting { get; set; }
        }
        public class RateLimitRule
        {
            public string Endpoint { get; set; }
            public string Period { get; set; }
            public long Limit { get; set; }
        }
    }
}
