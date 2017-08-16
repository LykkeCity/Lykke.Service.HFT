using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IMatchingEngineAdapter
    {
		bool IsConnected { get; }
		
		Task CancelLimitOrderAsync(string limitOrderId);
		Task CashInOutAsync(string clientId, string assetId, double amount);
		Task HandleMarketOrderAsync(string clientId, string assetPairId, OrderAction orderAction, double volume, bool straight, double? reservedLimitVolume = default(double?));
		Task PlaceLimitOrderAsync(string clientId, string assetPairId, OrderAction orderAction, double volume, double price, bool cancelPreviousOrders = false);
		Task SwapAsync(string clientId1, string assetId1, double amount1, string clientId2, string assetId2, double amount2);
		Task TransferAsync(string fromClientId, string toClientId, string assetId, double amount);
		Task UpdateBalanceAsync(string clientId, string assetId, double value);
	}
}
