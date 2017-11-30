using WampSharp.V2.Authentication;
using WampSharp.V2.Client;
using WampSharp.V2.Core.Contracts;
//using Konscious.Security.Cryptography;

namespace Lykke.Service.HFT.Wamp.Client
{
    public class TicketAuthenticator : IWampClientAuthenticator
    {
        //var hashAlgorithm = new HMACBlake2B(128);
        //hashAlgorithm.Initialize();
        //var subscriptionId = hashAlgorithm.ComputeHash(authId.ToUtf8Bytes()).ToBase64();

        private static readonly string[] AuthMethods = { "ticket" };

        public TicketAuthenticator(string authId)
        {
            AuthenticationId = authId;
        }

        public AuthenticationResponse Authenticate(string authmethod, ChallengeDetails extra)
        {
            if (authmethod != "ticket")
            {
                throw new WampAuthenticationException("don't know how to authenticate using '" + authmethod + "'");
            }
            // 'Signature' is ignored
            return new AuthenticationResponse();
        }

        public string[] AuthenticationMethods => AuthMethods;

        public string AuthenticationId { get; }
    }
}
