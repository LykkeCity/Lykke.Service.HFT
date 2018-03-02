using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using WampSharp.V2.Authentication;

namespace Lykke.Service.HFT.Wamp.Security
{
    public class WampSessionAuthenticatorFactory : IWampSessionAuthenticatorFactory
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IApiKeyValidator _apiKeyValidator;
        public WampSessionAuthenticatorFactory(
            [NotNull] IApiKeyValidator apiKeyValidator,
            [NotNull] ISessionRepository sessionRepository)
        {
            _apiKeyValidator = apiKeyValidator ?? throw new ArgumentNullException(nameof(apiKeyValidator));
            _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        }

        public IWampSessionAuthenticator GetSessionAuthenticator(WampPendingClientDetails details, IWampSessionAuthenticator transportAuthenticator)
        {
            if (details.HelloDetails.AuthenticationMethods.Contains(AuthMethods.Ticket))
            {
                return new TicketSessionAuthenticator(details, _apiKeyValidator, _sessionRepository);
            }
            return new AnonymousWampSessionAuthenticator();
        }
    }
}