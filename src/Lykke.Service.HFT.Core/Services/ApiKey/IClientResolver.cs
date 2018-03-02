using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services.ApiKey
{
    public interface IClientResolver
    {
        Task<string> GetClientAsync(string apiKey);
    }
}
