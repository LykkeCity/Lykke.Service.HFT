using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core.Services.ApiKey;
using WampSharp.V2.Authentication;

namespace Lykke.Service.HFT.Wamp.Security
{
    public class WampSessionAuthenticatorFactory : IWampSessionAuthenticatorFactory
    {
        private readonly ISessionCache _sessionCache;
        private readonly IApiKeyValidator _apiKeyValidator;
        public WampSessionAuthenticatorFactory(
            [NotNull] IApiKeyValidator apiKeyValidator,
            [NotNull] ISessionCache sessionCache)
        {
            _apiKeyValidator = apiKeyValidator ?? throw new ArgumentNullException(nameof(apiKeyValidator));
            _sessionCache = sessionCache ?? throw new ArgumentNullException(nameof(sessionCache));
        }

        public IWampSessionAuthenticator GetSessionAuthenticator(WampPendingClientDetails details, IWampSessionAuthenticator transportAuthenticator)
        {
            if (details.HelloDetails.AuthenticationMethods.Contains(AuthMethods.Ticket))
            {
                return new TicketSessionAuthenticator(details, _apiKeyValidator, _sessionCache);
            }
            return new AnonymousWampSessionAuthenticator();
        }
    }
}