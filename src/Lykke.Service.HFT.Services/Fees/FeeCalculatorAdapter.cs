using System;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using FeeOrderAction = Lykke.Service.FeeCalculator.AutorestClient.Models.OrderAction;
using FeeType = Lykke.Service.FeeCalculator.AutorestClient.Models.FeeType;
using OrderAction = Lykke.Service.HFT.Core.Domain.OrderAction;

namespace Lykke.Service.HFT.Services.Fees
{
    public class FeeCalculatorAdapter : IFeeCalculatorAdapter
    {
        private readonly IFeeCalculatorClient _feeCalculatorClient;
        private readonly FeeSettings _feeSettings;
        private readonly LimitOrderFeeCache _cache;

        public FeeCalculatorAdapter(IFeeCalculatorClient feeCalculatorClient, FeeSettings feeSettings, LimitOrderFeeCache cache)
        {
            _feeCalculatorClient = feeCalculatorClient ?? throw new ArgumentNullException(nameof(feeCalculatorClient));
            _feeSettings = feeSettings ?? throw new ArgumentNullException(nameof(feeSettings));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<MarketOrderFeeModel[]> GetMarketOrderFees(string clientId, AssetPair assetPair, OrderAction orderAction)
        {
            var fee = await _feeCalculatorClient.GetMarketOrderAssetFee(clientId, assetPair.Id, assetPair.BaseAssetId, ToFeeOrderAction(orderAction));

            return new[]
            {
                new MarketOrderFeeModel
                {
                    Size = (double) fee.Amount,
                    SizeType = GetFeeSizeType(fee.Type),
                    SourceClientId = clientId,
                    TargetClientId = fee.TargetWalletId ?? _feeSettings.TargetClientId.Hft,
                    Type = fee.Amount == 0m
                        ? (int) MarketOrderFeeType.NO_FEE
                        : (int) MarketOrderFeeType.CLIENT_FEE,
                    AssetId = string.IsNullOrEmpty(fee.TargetAssetId)
                        ? Array.Empty<string>()
                        : new[] {fee.TargetAssetId}
                }
            };
        }

        public async Task<LimitOrderFeeModel[]> GetLimitOrderFees(string clientId, AssetPair assetPair, OrderAction orderAction)
        {
            // Check if fee is available in the cache otherwise request it from the calculator.
            var cachedFee = await _cache.TryGetLimitOrderFee(clientId, assetPair, orderAction);
            if (cachedFee != null)
            {
                return new[] { cachedFee };
            }

            var fee = await _feeCalculatorClient.GetLimitOrderFees(clientId, assetPair.Id, assetPair.BaseAssetId, ToFeeOrderAction(orderAction));

            var model = new LimitOrderFeeModel
            {
                MakerSize = (double)fee.MakerFeeSize,
                TakerSize = (double)fee.TakerFeeSize,
                SourceClientId = clientId,
                TargetClientId = _feeSettings.TargetClientId.Hft,
                Type = fee.MakerFeeSize == 0m && fee.TakerFeeSize == 0m
                    ? (int)LimitOrderFeeType.NO_FEE
                    : (int)LimitOrderFeeType.CLIENT_FEE,
                MakerFeeModificator = (double)fee.MakerFeeModificator,
                MakerSizeType = GetFeeSizeType(fee.MakerFeeType),
                TakerSizeType = GetFeeSizeType(fee.TakerFeeType)
            };

            // Add to cache
            _cache.CacheLimitOrderFee(clientId, assetPair, orderAction, model);

            return new[] { model };
        }

        private static int GetFeeSizeType(FeeType type)
        {
            var sizeType = type == FeeType.Absolute
                ? FeeSizeType.ABSOLUTE
                : FeeSizeType.PERCENTAGE;

            return (int)sizeType;
        }

        private static FeeOrderAction ToFeeOrderAction(OrderAction action)
        {
            FeeOrderAction orderAction;
            switch (action)
            {
                case OrderAction.Buy:
                    orderAction = FeeOrderAction.Buy;
                    break;
                case OrderAction.Sell:
                    orderAction = FeeOrderAction.Sell;
                    break;
                default:
                    throw new InvalidOperationException("Unknown order action");
            }

            return orderAction;
        }
    }
}