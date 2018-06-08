using System;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Controllers
{
    public class RequestValidator
    {
        private readonly AppSettings.HighFrequencyTradingSettings _appSettings;
        public RequestValidator(AppSettings.HighFrequencyTradingSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public bool ValidateAssetPair(string assetPairId, Assets.Client.Models.AssetPair assetPair, out ResponseModel model)
        {
            if (assetPair == null)
            {
                model = ResponseModel.CreateFail(ResponseModel.ErrorCodeType.UnknownAsset);
                return false;
            }
            if (IsAssetPairDisabled(assetPair))
            {
                model = ResponseModel.CreateInvalidFieldError("AssetPairId", $"AssetPair {assetPairId} is temporarily disabled");
                return false;
            }

            model = null;
            return true;
        }

        public bool ValidateVolume(double volume, double minVolume, string asset, out ResponseModel model)
        {
            if (Math.Abs(volume) < double.Epsilon || Math.Abs(volume) < minVolume)
            {
                model = ResponseModel.CreateFail(ResponseModel.ErrorCodeType.Dust, $"The amount should be higher than minimal order size {minVolume} {asset}");
                return false;
            }

            model = null;
            return true;
        }

        public bool ValidatePrice(double price, out ResponseModel model)
        {
            if (Math.Abs(price) < double.Epsilon)
            {
                model = ResponseModel.CreateInvalidFieldError("Price", "Price must be greater than asset accuracy.");
                return false;
            }

            model = null;
            return true;
        }

        public bool ValidateAsset(Assets.Client.Models.AssetPair assetPair, string assetId,
            Assets.Client.Models.Asset baseAsset, Assets.Client.Models.Asset quotingAsset, out ResponseModel model)
        {
            if (assetId != baseAsset.Id && assetId != baseAsset.DisplayId && assetId != quotingAsset.Id && assetId != quotingAsset.DisplayId)
            {
                model = ResponseModel.CreateInvalidFieldError("Asset", $"Asset <{assetId}> is not valid for asset pair <{assetPair.Id}>.");
                return false;
            }

            model = null;
            return true;
        }

        private bool IsAssetPairDisabled(Assets.Client.Models.AssetPair assetPair)
        {
            return IsAssetDisabled(assetPair.BaseAssetId) || IsAssetDisabled(assetPair.QuotingAssetId);
        }

        private bool IsAssetDisabled(string asset)
        {
            return _appSettings.DisabledAssets.Contains(asset);
        }
    }
}
