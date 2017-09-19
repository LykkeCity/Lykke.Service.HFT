using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services.ApiKey
{
    public interface IApiKeyCacheInitializer
    {
        Task InitApiKeyCache();
    }
}
