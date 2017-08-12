using System.Threading.Tasks;

namespace Lykke.Service.HFT.Abstractions.Services
{
	public interface IApiKeyValidator
	{
		Task<bool> ValidateAsync(string apiKey);
	}
}
