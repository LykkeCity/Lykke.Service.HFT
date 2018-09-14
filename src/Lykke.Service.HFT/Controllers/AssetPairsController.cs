﻿using AssetsCache;
using AssetsCache.ReadModels;
using Lykke.Service.HFT.Contracts.Assets;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Controllers
{
    /// <summary>
    /// Controller for asset pair operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AssetPairsController : Controller
    {
        private readonly IAssetPairsReadModel _assetPairsReadModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetPairsController"/> class.
        /// </summary>
        public AssetPairsController(IAssetPairsReadModel assetPairsReadModel)
        {
            _assetPairsReadModel = assetPairsReadModel;
        }

        /// <summary>
        /// Get all enabled asset pairs.
        /// </summary>
        /// <response code="200">All enabled asset pairs.</response>
        [HttpGet]
        [SwaggerOperation(nameof(GetAssetPairs))]
        [ProducesResponseType(typeof(IEnumerable<AssetPairModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAssetPairs()
        {
            var assetPairs = _assetPairsReadModel.GetAll();
            return Ok(assetPairs.Select(ToModel));
        }

        /// <summary>
        /// Get specified asset pair.
        /// </summary>
        /// <param name="id">Asset pair ID. Example: AUDUSD</param>
        /// <response code="200">Specified asset pair.</response>
        /// <response code="404">No asset pair with given id found or asset pair is disabled.</response>
        [HttpGet("{id}")]
        [SwaggerOperation(nameof(GetAssetPair))]
        [ProducesResponseType(typeof(AssetPairModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAssetPair(string id)
        {
            var assetPair = _assetPairsReadModel.GetIfEnabled(id);
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
