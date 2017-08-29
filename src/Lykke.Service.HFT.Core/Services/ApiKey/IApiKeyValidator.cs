using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services.ApiKey
{
	public interface IApiKeyValidator
	{
		Task<bool> ValidateAsync(string apiKey);
	}
}
