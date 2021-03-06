﻿using Lykke.Service.Assets.Client.Models.v3;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Core.Settings;
using System;

namespace Lykke.Service.HFT.Controllers
{
    /// <summary>
    /// Request parameter validation logic.
    /// </summary>
    public class RequestValidator
    {
        private readonly HighFrequencyTradingSettings _appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestValidator"/> class.
        /// </summary>
        /// <param name="appSettings">The application settings.</param>
        public RequestValidator(HighFrequencyTradingSettings appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// Validate requested asset pair.
        /// </summary>
        public bool ValidateAssetPair(string assetPairId, AssetPair assetPair, out ResponseModel model)
        {
            if (assetPair == null)
            {
                model = ResponseModel.CreateInvalidFieldError("AssetPairId", $"AssetPair {assetPairId} is unknown");
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

        /// <summary>
        /// Validate requested volume.
        /// </summary>
        public bool ValidateVolume(decimal volume, decimal minVolume, string asset, out ResponseModel model)
        {
            if (Math.Abs(volume) < minVolume)
            {
                model = ResponseModel.CreateFail(ErrorCodeType.Rejected, $"The amount should be higher than minimal order size {minVolume} {asset}");
                return false;
            }

            model = null;
            return true;
        }

        /// <summary>
        /// Validate requested price.
        /// </summary>
        public bool ValidatePrice(decimal price, out ResponseModel model, string name = "Price")
        {
            if (price <= 0)
            {
                model = ResponseModel.CreateInvalidFieldError(name, "Price must be greater than asset accuracy.");
                return false;
            }

            model = null;
            return true;
        }

        /// <summary>
        /// Validate requested asset.
        /// </summary>
        public bool ValidateAsset(AssetPair assetPair, string assetId,
            Asset baseAsset, Asset quotingAsset, out ResponseModel model)
        {
            if (assetId != baseAsset.Id && assetId != baseAsset.DisplayId && assetId != quotingAsset.Id && assetId != quotingAsset.DisplayId)
            {
                model = ResponseModel.CreateInvalidFieldError("Asset", $"Asset <{assetId}> is not valid for asset pair <{assetPair.Id}>.");
                return false;
            }

            model = null;
            return true;
        }

        private bool IsAssetPairDisabled(AssetPair assetPair)
        {
            return IsAssetDisabled(assetPair.BaseAssetId) || IsAssetDisabled(assetPair.QuotingAssetId);
        }

        private bool IsAssetDisabled(string asset)
        {
            return _appSettings.DisabledAssets.Contains(asset);
        }
    }
}
