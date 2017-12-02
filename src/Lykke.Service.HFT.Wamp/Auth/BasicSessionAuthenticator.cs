using System;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core.Services.ApiKey;
using WampSharp.V2.Authentication;
using WampSharp.V2.Core.Contracts;

namespace Lykke.Service.HFT.Wamp.Auth
{
    public class BasicSessionAuthenticator : WampSessionAuthenticator
    {
        private readonly IClientResolver _clientResolver;
        private readonly IApiKeyValidator _apiKeyValidator;
        private readonly WampPendingClientDetails _details;
        private bool _isAuthenticated;

        public BasicSessionAuthenticator(
            [NotNull] WampPendingClientDetails details,
            [NotNull] IApiKeyValidator apiKeyValidator,
            [NotNull] IClientResolver clientResolver)
        {
            _details = details ?? throw new ArgumentNullException(nameof(details));
            _apiKeyValidator = apiKeyValidator ?? throw new ArgumentNullException(nameof(apiKeyValidator));
            _clientResolver = clientResolver ?? throw new ArgumentNullException(nameof(clientResolver));

            AuthenticationId = details.HelloDetails.AuthenticationId;
        }

        public override void Authenticate(string signature, AuthenticateExtraData extra)
        {
            var apiKey = AuthenticationId;
            if (_apiKeyValidator.ValidateAsync(apiKey).Result)
            {
                _clientResolver.SetNotificationIdAsync(apiKey, _details.SessionId.ToString()).Wait();
                _isAuthenticated = true;

                WelcomeDetails = new WelcomeDetails
                {
                    AuthenticationProvider = "static",
                    AuthenticationRole = "HFT client"
                };

                Authorizer = HftClientStaticAuthorizer.Instance;
            }
        }

        public override bool IsAuthenticated
        {
            get
            {
                if (!_isAuthenticated)
                {
                    Authenticate(null, null);
                }
                return _isAuthenticated;
            }
        }

        public override string AuthenticationId { get; }

        public override string AuthenticationMethod => AuthMethods.Basic;
    }
}