using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services
{
	public interface IApiKeyValidator
	{
		Task<bool> ValidateAsync(string apiKey);
	}
}
