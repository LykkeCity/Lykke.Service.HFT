using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Service.HFT.Middleware
{
    public class KeyAuthHandler : AuthenticationHandler<KeyAuthOptions>
    {
        private readonly IApiKeyValidator _apiKeyValidator;
        private readonly IClientResolver _clientResolver;

        public KeyAuthHandler(IOptionsMonitor<KeyAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            IApiKeyValidator apiKeyValidator, IClientResolver clientResolver)
            : base(options, logger, encoder, clock)
        {
            _apiKeyValidator = apiKeyValidator;
            _clientResolver = clientResolver;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.TryGetValue(KeyAuthOptions.DefaultHeaderName, out var headerValue))
            {
                return AuthenticateResult.Fail("No api key header.");
            }

            var apiKey = headerValue.First();
            if (!(await _apiKeyValidator.ValidateAsync(apiKey)))
            {
                return AuthenticateResult.Fail("Invalid API key.");
            }

            var authenticationScheme = "apikey";
            var identity = new ClaimsIdentity(authenticationScheme);
            var client = await _clientResolver.GetClientAsync(apiKey);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, client));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, authenticationScheme);
            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
