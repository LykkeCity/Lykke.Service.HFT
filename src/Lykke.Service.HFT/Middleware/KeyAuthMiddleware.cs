using System.Text.Encodings.Web;
using Lykke.Service.HFT.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Service.HFT.Middleware
{
	public class KeyAuthMiddleware : AuthenticationMiddleware<KeyAuthOptions>
	{
		private readonly IApiKeyValidator _apiKeyValidator;
		private readonly IClientResolver _clientResolver;

		public KeyAuthMiddleware(
			IApiKeyValidator apiKeyValidator,
			IClientResolver clientResolver,
			RequestDelegate next,
			IOptions<KeyAuthOptions> options,
			ILoggerFactory loggerFactory,
			UrlEncoder encoder) : base(next, options, loggerFactory, encoder)
		{
			_apiKeyValidator = apiKeyValidator;
			_clientResolver = clientResolver;
		}

		protected override AuthenticationHandler<KeyAuthOptions> CreateHandler()
		{
			return new KeyAuthHandler(_apiKeyValidator, _clientResolver);
		}
	}
}
