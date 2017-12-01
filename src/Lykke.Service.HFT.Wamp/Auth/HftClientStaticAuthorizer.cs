using WampSharp.V2.Authentication;
using WampSharp.V2.Core.Contracts;

namespace Lykke.Service.HFT.Wamp.Auth
{
    public class HftClientStaticAuthorizer : IWampAuthorizer
    {
        public static HftClientStaticAuthorizer Instance = new HftClientStaticAuthorizer();

        public bool CanRegister(RegisterOptions options, string procedure)
        {
            return false;
        }

        public bool CanCall(CallOptions options, string procedure)
        {
            return true;
        }

        public bool CanPublish(PublishOptions options, string topicUri)
        {
            return false;
        }

        public bool CanSubscribe(SubscribeOptions options, string topicUri)
        {
            var isExactTopicName = string.IsNullOrEmpty(options?.Match) || options.Match == WampMatchPattern.Exact;
            if (!isExactTopicName)
                return false;   //throw new WampAuthenticationException();

            return true;
        }
    }
}