using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.Assets.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;

namespace Lykke.Service.HFT.Services
{
    public class MatchingEngineAdapter : IMatchingEngineAdapter
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IRepository<LimitOrderState> _orderStateRepository;
        private readonly IFeeCalculatorClient _feeCalculatorClient;
        private readonly IAssetsService _assetsService;
        private readonly FeesSettings _feesSettings;

        private readonly Dictionary<MeStatusCodes, ResponseModel.ErrorCodeType> _statusCodesMap = new Dictionary<MeStatusCodes, ResponseModel.ErrorCodeType>
        {
            {MeStatusCodes.Ok, ResponseModel.ErrorCodeType.Ok},
            {MeStatusCodes.LowBalance, ResponseModel.ErrorCodeType.LowBalance},
            {MeStatusCodes.AlreadyProcessed, ResponseModel.ErrorCodeType.AlreadyProcessed},
            {MeStatusCodes.UnknownAsset, ResponseModel.ErrorCodeType.UnknownAsset},
            {MeStatusCodes.NoLiquidity, ResponseModel.ErrorCodeType.NoLiquidity},
            {MeStatusCodes.NotEnoughFunds, ResponseModel.ErrorCodeType.NotEnoughFunds},
            {MeStatusCodes.Dust, ResponseModel.ErrorCodeType.Dust},
            {MeStatusCodes.ReservedVolumeHigherThanBalance, ResponseModel.ErrorCodeType.ReservedVolumeHigherThanBalance},
            {MeStatusCodes.NotFound, ResponseModel.ErrorCodeType.NotFound},
            {MeStatusCodes.BalanceLowerThanReserved, ResponseModel.ErrorCodeType.BalanceLowerThanReserved},
            {MeStatusCodes.LeadToNegativeSpread, ResponseModel.ErrorCodeType.LeadToNegativeSpread},
            {MeStatusCodes.Runtime, ResponseModel.ErrorCodeType.RuntimeError}
        };


        public MatchingEngineAdapter(IMatchingEngineClient matchingEngineClient,
            IRepository<LimitOrderState> orderStateRepository, IFeeCalculatorClient feeCalculatorClient,
            IAssetsService assetsService, FeesSettings feesSettings)
        {
            _matchingEngineClient =
                matchingEngineClient ?? throw new ArgumentNullException(nameof(matchingEngineClient));
            _orderStateRepository =
                orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _feeCalculatorClient = feeCalculatorClient ?? throw new ArgumentNullException(nameof(feeCalculatorClient));
            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _feesSettings = feesSettings ?? throw new ArgumentNullException(nameof(feesSettings));
        }

        public async Task<ResponseModel> CancelLimitOrderAsync(Guid limitOrderId)
        {
            var response = await _matchingEngineClient.CancelLimitOrderAsync(limitOrderId.ToString());
            return ConvertToApiModel(response.Status);
        }

        public async Task<ResponseModel<double>> HandleMarketOrderAsync(string clientId, string assetPairId, Core.Domain.OrderAction orderAction, double volume,
            bool straight, double? reservedLimitVolume = null)
        {
            var order = new MarketOrderModel
            {
                Id = GetNextRequestId().ToString(),
                AssetPairId = assetPairId,
                ClientId = clientId,
                ReservedLimitVolume = reservedLimitVolume,
                Straight = straight,
                Volume = Math.Abs(volume),
                OrderAction = orderAction.ToMeOrderAction(),
                Fee = await GetMarketOrderFee(clientId, assetPairId, orderAction)
            };

            var response = await _matchingEngineClient.HandleMarketOrderAsync(order);
            if (response.Status == MeStatusCodes.Ok)
            {
                return ResponseModel<double>.CreateOk(response.Price);
            }
            return ConvertToApiModel<double>(response.Status);
        }

        public async Task<ResponseModel<Guid>> PlaceLimitOrderAsync(string clientId, string assetPairId, Core.Domain.OrderAction orderAction, double volume,
            double price, bool cancelPreviousOrders = false)
        {
            var requestId = GetNextRequestId();

            await _orderStateRepository.Add(new LimitOrderState { Id = requestId, ClientId = clientId, AssetPairId = assetPairId, Volume = volume, Price = price });

            var order = new LimitOrderModel
            {
                Id = requestId.ToString(),
                AssetPairId = assetPairId,
                ClientId = clientId,
                Price = price,
                CancelPreviousOrders = cancelPreviousOrders,
                Volume = Math.Abs(volume),
                OrderAction = orderAction.ToMeOrderAction(),
                Fee = await GetLimitOrderFee(clientId, assetPairId, orderAction)
            };

            var response = await _matchingEngineClient.PlaceLimitOrderAsync(order);
            if (response.Status == MeStatusCodes.Ok)
            {
                return ResponseModel<Guid>.CreateOk(requestId);
            }
            return ConvertToApiModel<Guid>(response.Status);
        }

        private Guid GetNextRequestId()
        {
            return Guid.NewGuid();
        }

        private ResponseModel ConvertToApiModel(MeStatusCodes status)
        {
            if (status == MeStatusCodes.Ok)
                return ResponseModel.CreateOk();

            return ResponseModel.CreateFail(_statusCodesMap[status]);
        }

        private ResponseModel<T> ConvertToApiModel<T>(MeStatusCodes status)
        {
            return ResponseModel<T>.CreateFail(_statusCodesMap[status]);
        }

        private async Task<MarketOrderFeeModel> GetMarketOrderFee(string clientId, string assetPairId, Core.Domain.OrderAction orderAction)
        {
            var assetPair = await _assetsService.AssetPairGetAsync(assetPairId);
            var fee = await _feeCalculatorClient.GetMarketOrderFees(clientId, assetPairId, assetPair?.BaseAssetId,
                orderAction.ToFeeOrderAction());

            return new MarketOrderFeeModel
            {
                Size = (double)fee.DefaultFeeSize,
                SourceClientId = clientId,
                TargetClientId = _feesSettings.TargetClientId,
                Type = (int)MarketOrderFeeType.CLIENT_FEE
            };
        }

        private async Task<LimitOrderFeeModel> GetLimitOrderFee(string clientId, string assetPairId, Core.Domain.OrderAction orderAction)
        {
            var assetPair = await _assetsService.AssetPairGetAsync(assetPairId);
            var fee = await _feeCalculatorClient.GetLimitOrderFees(clientId, assetPairId, assetPair?.BaseAssetId, orderAction.ToFeeOrderAction());

            return new LimitOrderFeeModel
            {
                MakerSize = (double)fee.MakerFeeSize,
                TakerSize = (double)fee.TakerFeeSize,
                SourceClientId = clientId,
                TargetClientId = _feesSettings.TargetClientId,
                Type = (int)LimitOrderFeeType.CLIENT_FEE
            };
        }
    }
}
