using System.Threading.Tasks;

namespace Lykke.Service.HFT.Abstractions.Services
{
	public interface IApiKeyGenerator
	{
		Task<string> GenerateApiKeyAsync(string clientId);
	}
}
