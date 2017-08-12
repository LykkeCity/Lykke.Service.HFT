using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;
using Lykke.Service.HFT.Abstractions.Services;

namespace Lykke.Service.HFT.WebApi.Middleware
{
	public class KeyAuthHandler : AuthenticationHandler<KeyAuthOptions>
	{
		private readonly IApiKeyValidator _apiKeyValidator;
		private readonly IClientResolver _clientResolver;

		public KeyAuthHandler(
			IApiKeyValidator apiKeyValidator,
			IClientResolver clientResolver)
		{
			_apiKeyValidator = apiKeyValidator;
			_clientResolver = clientResolver;
		}
		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			StringValues headerValue;

			if (!Context.Request.Headers.TryGetValue(Options.KeyHeaderName, out headerValue))
			{
				return AuthenticateResult.Fail("No api key header.");
			}

			var apiKey = headerValue.First();
			if (!(await _apiKeyValidator.ValidateAsync(apiKey)))
			{
				return AuthenticateResult.Fail("Invalid API key.");
			}

			var identity = new ClaimsIdentity("apikey");
			var client = await _clientResolver.GetClientAsync(apiKey);
			identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, client));
			var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, "apikey");
			return await Task.FromResult(AuthenticateResult.Success(ticket));
		}
	}
}
