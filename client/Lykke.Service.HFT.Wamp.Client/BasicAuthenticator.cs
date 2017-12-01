using WampSharp.V2.Authentication;
using WampSharp.V2.Client;
using WampSharp.V2.Core.Contracts;

namespace Lykke.Service.HFT.Wamp.Client
{
    public class BasicAuthenticator : IWampClientAuthenticator
    {
        private const string AuthMethod = "anonymous";
        private static readonly string[] AuthMethods = { AuthMethod };

        public BasicAuthenticator(string authId)
        {
            AuthenticationId = authId;
        }

        public AuthenticationResponse Authenticate(string authmethod, ChallengeDetails extra)
        {
            if (authmethod != AuthMethod)
            {
                throw new WampAuthenticationException("don't know how to authenticate using '" + authmethod + "'");
            }
            return new AuthenticationResponse();
        }

        public string[] AuthenticationMethods => AuthMethods;

        public string AuthenticationId { get; }
    }
}
