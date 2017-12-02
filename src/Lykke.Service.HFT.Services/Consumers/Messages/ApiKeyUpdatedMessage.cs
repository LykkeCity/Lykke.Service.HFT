using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Services.Consumers.Messages
{
    public class ApiKeyUpdatedMessage
    {
        public ApiKey ApiKey { get; set; }
    }
}
