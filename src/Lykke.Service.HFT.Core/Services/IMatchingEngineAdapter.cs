using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IMatchingEngineAdapter
    {
		bool IsConnected { get; }

	    Task PlaceLimitOrderAsync(string clientId, string assetPairId, string orderAction, double volume, double price);
    }
}
