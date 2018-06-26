using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.Service.Assets.Client.Models;
using OrderAction = Lykke.Service.HFT.Contracts.Orders.OrderAction;

namespace Lykke.Service.HFT.Core.Services
{
    /// <summary>
    /// Adaptor service for the FeeCalculator used in HFT.
    /// </summary>
    public interface IFeeCalculatorAdapter
    {
        /// <summary>
        /// Calculate the fees for a limit order.
        /// </summary>
        /// <param name="clientId">the client id</param>
        /// <param name="assetPair">the asset-pair</param>
        /// <param name="orderAction">the action (Buy/Sell)</param>
        /// <returns></returns>
        Task<LimitOrderFeeModel[]> GetLimitOrderFees(string clientId, AssetPair assetPair, OrderAction orderAction);

        /// <summary>
        /// Calculate the fees for a market order.
        /// </summary>
        /// <param name="clientId">the client id</param>
        /// <param name="assetPair">the asset-pair</param>
        /// <param name="orderAction">the action (Buy/Sell)</param>
        /// <returns></returns>
        Task<MarketOrderFeeModel[]> GetMarketOrderFees(string clientId, AssetPair assetPair, OrderAction orderAction);
    }
}