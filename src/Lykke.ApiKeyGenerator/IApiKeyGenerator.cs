using System.Threading.Tasks;

namespace Lykke.ApiKeyGenerator
{
	public interface IApiKeyGenerator
	{
		Task<string> GenerateApiKeyAsync(string clientId);
	}
}
