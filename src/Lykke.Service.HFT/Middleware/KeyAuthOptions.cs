using Microsoft.AspNetCore.Authentication;

namespace Lykke.Service.HFT.Middleware
{
    internal class KeyAuthOptions : AuthenticationSchemeOptions
    {
        public const string DefaultHeaderName = "api-key";
        public const string AuthenticationScheme = "Automatic";
    }
}
