using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core.Services.ApiKey;
using WampSharp.V2.Authentication;

namespace Lykke.Service.HFT.Wamp.Auth
{
    public class WampSessionAuthenticatorFactory : IWampSessionAuthenticatorFactory
    {
        private readonly IClientResolver _clientResolver;
        private readonly IApiKeyValidator _apiKeyValidator;
        public WampSessionAuthenticatorFactory(
            [NotNull] IApiKeyValidator apiKeyValidator,
            [NotNull] IClientResolver clientResolver)
        {
            _apiKeyValidator = apiKeyValidator ?? throw new ArgumentNullException(nameof(apiKeyValidator));
            _clientResolver = clientResolver ?? throw new ArgumentNullException(nameof(clientResolver));
        }

        public IWampSessionAuthenticator GetSessionAuthenticator(WampPendingClientDetails details, IWampSessionAuthenticator transportAuthenticator)
        {
            if (details.HelloDetails.AuthenticationMethods.Contains(AuthMethods.Ticket))
            {
                return new TicketSessionAuthenticator(details, _apiKeyValidator, _clientResolver);
            }
            return new BasicSessionAuthenticator(details, _apiKeyValidator, _clientResolver);
        }
    }
}