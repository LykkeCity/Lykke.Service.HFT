using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services
{
	public interface IApiKeyGenerator
	{
		Task<string> GenerateApiKeyAsync(string clientId);
	}
}
