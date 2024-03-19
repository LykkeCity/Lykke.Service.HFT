using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Service.HFT.Middleware
{
    [UsedImplicitly]
    internal class KeyAuthHandler : AuthenticationHandler<KeyAuthOptions>
    {
        private readonly IApiKeysCacheService _apiKeysCacheService;

        public KeyAuthHandler(IOptionsMonitor<KeyAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            IApiKeysCacheService apiKeysCacheService)
            : base(options, logger, encoder, clock)
        {
            _apiKeysCacheService = apiKeysCacheService;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.TryGetValue(KeyAuthOptions.DefaultHeaderName, out var headerValue))
            {
                return AuthenticateResult.NoResult();
            }

            var apiKey = headerValue.First();
            var walletId = await _apiKeysCacheService.GetWalletIdAsync(apiKey);

            if (walletId == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(10)); // todo: ban requests from IPs with 401 response.
                return AuthenticateResult.Fail("Invalid API key.");
            }

            var authenticationScheme = "apikey";
            var identity = new ClaimsIdentity(authenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, walletId));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, authenticationScheme);
            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
