using Microsoft.AspNetCore.Authentication;

namespace Lykke.Service.HFT.Middleware
{
    internal class HmacAuthOptions : AuthenticationSchemeOptions
    {
        public const string DefaultHeaderName = "Authorization";
        public const string AuthenticationScheme = "HMAC";

        public enum FailReason
        {
            InvalidHeader,
            RequestExpired,
            InvalidApiKey,
            RegenerateKey
        }
    }
}
