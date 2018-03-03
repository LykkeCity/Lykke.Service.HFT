using WampSharp.V2.Authentication;
using WampSharp.V2.Core.Contracts;

namespace Lykke.Service.HFT.Wamp.Security
{
    public class AnonymousWampSessionAuthenticator : WampSessionAuthenticator
    {
        private readonly IWampAuthorizer _authorizer;

        public AnonymousWampSessionAuthenticator()
        {
            _authorizer = AnonymousAuthorizer.Instance;
        }

        public override void Authenticate(string signature, AuthenticateExtraData extra)
        {
        }

        public override IWampAuthorizer Authorizer => _authorizer;

        public override bool IsAuthenticated => true;

        public override string AuthenticationId => "Anonymous";

        public override string AuthenticationMethod => AuthMethods.Anonymous;
    }
}
