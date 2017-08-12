using System.Threading.Tasks;

namespace Lykke.Service.HFT.Abstractions.Services
{
	public interface IClientResolver
	{
		Task<string> GetClientAsync(string apiKey);
	}
}
