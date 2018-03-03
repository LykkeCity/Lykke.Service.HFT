using System;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using WampSharp.V2.Authentication;
using WampSharp.V2.Core.Contracts;

namespace Lykke.Service.HFT.Wamp.Security
{
    public class TicketSessionAuthenticator : WampSessionAuthenticator
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IApiKeyValidator _apiKeyValidator;
        private readonly WampPendingClientDetails _details;

        public TicketSessionAuthenticator(
            [NotNull] WampPendingClientDetails details,
            [NotNull] IApiKeyValidator apiKeyValidator,
            [NotNull] ISessionRepository sessionRepository)
        {
            _details = details ?? throw new ArgumentNullException(nameof(details));
            _apiKeyValidator = apiKeyValidator ?? throw new ArgumentNullException(nameof(apiKeyValidator));
            _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));

            AuthenticationId = details.HelloDetails.AuthenticationId;
        }

        public override void Authenticate(string signature, AuthenticateExtraData extra)
        {
            if (_apiKeyValidator.ValidateAsync(signature).Result)
            {
                _sessionRepository.AddSessionId(signature, _details.SessionId);

                IsAuthenticated = true;

                WelcomeDetails = new WelcomeDetails
                {
                    AuthenticationRole = "HFT client"
                };

                Authorizer = TokenAuthorizer.Instance;
            }
        }

        public override string AuthenticationId { get; }

        public override string AuthenticationMethod => AuthMethods.Ticket;
    }
}