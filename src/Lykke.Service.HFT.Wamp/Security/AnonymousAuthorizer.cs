using System.Linq;
using WampSharp.V2.Authentication;
using WampSharp.V2.Core.Contracts;

namespace Lykke.Service.HFT.Wamp.Security
{
    public class AnonymousAuthorizer : IWampAuthorizer
    {
        public static AnonymousAuthorizer Instance = new AnonymousAuthorizer();

        public bool CanRegister(RegisterOptions options, string procedure) => false;

        public bool CanCall(CallOptions options, string procedure) => false;

        public bool CanPublish(PublishOptions options, string topicUri) => false;

        public bool CanSubscribe(SubscribeOptions options, string topicUri) => Topics.WithAuth.All(item => item != topicUri);
    }
}
