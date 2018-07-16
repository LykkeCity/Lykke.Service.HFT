using System;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.MatchingEngine.Connector.Models.Common;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using FeeOrderAction = Lykke.Service.FeeCalculator.AutorestClient.Models.OrderAction;
using FeeType = Lykke.Service.FeeCalculator.AutorestClient.Models.FeeType;
using OrderAction = Lykke.Service.HFT.Contracts.Orders.OrderAction;
using MeFeeType = Lykke.MatchingEngine.Connector.Models.Common.FeeType;

namespace Lykke.Service.HFT.Services.Fees
{
    public class FeeCalculatorAdapter : IFeeCalculatorAdapter
    {
        private readonly IFeeCalculatorClient _feeCalculatorClient;
        private readonly FeeSettings _feeSettings;

        public FeeCalculatorAdapter(IFeeCalculatorClient feeCalculatorClient, FeeSettings feeSettings)
        {
            _feeCalculatorClient = feeCalculatorClient ?? throw new ArgumentNullException(nameof(feeCalculatorClient));
            _feeSettings = feeSettings ?? throw new ArgumentNullException(nameof(feeSettings));
        }

        public async Task<MarketOrderFeeModel[]> GetMarketOrderFees(string clientId, AssetPair assetPair,
            OrderAction orderAction)
        {
            var fee = await _feeCalculatorClient.GetMarketOrderAssetFee(clientId, assetPair.Id, assetPair.BaseAssetId,
                ToFeeOrderAction(orderAction));

            var model = new MarketOrderFeeModel
            {
                Size = (double) fee.Amount,
                SizeType = GetFeeSizeType(fee.Type),
                SourceClientId = clientId,
                TargetClientId = fee.TargetWalletId ?? _feeSettings.TargetClientId.Hft,
                Type = fee.Amount == 0m
                    ? MeFeeType.NO_FEE
                    : MeFeeType.CLIENT_FEE,
                AssetId = string.IsNullOrEmpty(fee.TargetAssetId)
                    ? Array.Empty<string>()
                    : new[] {fee.TargetAssetId}
            };

            return new[] {model};
        }

        public async Task<LimitOrderFeeModel[]> GetLimitOrderFees(string clientId, AssetPair assetPair, OrderAction orderAction)
        {
            var fee = await _feeCalculatorClient.GetLimitOrderFees(clientId, assetPair.Id, assetPair.BaseAssetId, ToFeeOrderAction(orderAction));

            var model = new LimitOrderFeeModel
            {
                MakerSize = (double)fee.MakerFeeSize,
                TakerSize = (double)fee.TakerFeeSize,
                SourceClientId = clientId,
                TargetClientId = _feeSettings.TargetClientId.Hft,
                Type = fee.MakerFeeSize == 0m && fee.TakerFeeSize == 0m
                    ? MeFeeType.NO_FEE
                    : MeFeeType.CLIENT_FEE,
                MakerFeeModificator = (double)fee.MakerFeeModificator,
                MakerSizeType = GetFeeSizeType(fee.MakerFeeType),
                TakerSizeType = GetFeeSizeType(fee.TakerFeeType),
                AssetId = Array.Empty<string>()
            };

            return new[] { model };
        }

        private static FeeSizeType GetFeeSizeType(FeeType type)
            => type == FeeType.Absolute
                ? FeeSizeType.ABSOLUTE
                : FeeSizeType.PERCENTAGE;

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