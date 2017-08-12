using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Lykke.Service.HFT.WebApi.Middleware.Validator;

namespace Lykke.Service.HFT.WebApi.Middleware
{
    public class KeyAuthMiddleware : AuthenticationMiddleware<KeyAuthOptions>
    {
        private readonly IApiKeyValidator _apiKeyValidator;

        public KeyAuthMiddleware(
            IApiKeyValidator apiKeyValidator,
            RequestDelegate next, 
            IOptions<KeyAuthOptions> options, 
            ILoggerFactory loggerFactory, 
            UrlEncoder encoder) : base(next, options, loggerFactory, encoder)
        {
            _apiKeyValidator = apiKeyValidator;
        }

        protected override AuthenticationHandler<KeyAuthOptions> CreateHandler()
        {
            return new KeyAuthHandler(_apiKeyValidator);
        }
    }
}
