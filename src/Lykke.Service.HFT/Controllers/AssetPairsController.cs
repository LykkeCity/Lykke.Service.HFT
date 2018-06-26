using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Contracts.Assets;
using Lykke.Service.HFT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Controllers
{
    [Route("api/[controller]")]
    public class AssetPairsController : Controller
    {
        private readonly IAssetServiceDecorator _assetServiceDecorator;

        public AssetPairsController(IAssetServiceDecorator assetServiceDecorator)
        {
            _assetServiceDecorator = assetServiceDecorator ?? throw new ArgumentNullException(nameof(assetServiceDecorator));
        }

        /// <summary>
        /// Get all asset pairs.
        /// </summary>
        /// <returns>All asset pairs.</returns>
        [HttpGet]
        [SwaggerOperation("GetAssetPairs")]
        [ProducesResponseType(typeof(IEnumerable<AssetPairModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAssetPairs()
        {
            var assetPairs = await _assetServiceDecorator.GetAllEnabledAssetPairsAsync();
            return Ok(assetPairs.Select(ToModel));
        }

        /// <summary>
        /// Get specified asset pair.
        /// </summary>
        /// <param name="id">Asset pair ID. Example: AUDUSD</param>
        /// <returns>Specified asset pair.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation("GetAssetPair")]
        [ProducesResponseType(typeof(AssetPairModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAssetPair(string id)
        {
            var assetPair = await _assetServiceDecorator.GetEnabledAssetPairAsync(id);
            if (assetPair == null)
            {
                return NotFound();
            }
            return Ok(ToModel(assetPair));
        }

        private static AssetPairModel ToModel(AssetPair src)
        {
            if (src == null)
            {
                return null;
            }

            return new AssetPairModel
            {
                Id = src.Id,
                Name = src.Name,
                Accuracy = src.Accuracy,
                InvertedAccuracy = src.InvertedAccuracy,
                BaseAssetId = src.BaseAssetId,
                QuotingAssetId = src.QuotingAssetId,
                MinVolume = src.MinVolume,
                MinInvertedVolume = src.MinInvertedVolume
            };
        }
    }
}
