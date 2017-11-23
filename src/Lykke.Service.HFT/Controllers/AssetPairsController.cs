using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Models;
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
        [SwaggerOperation("AssetPairs")]
        [ProducesResponseType(typeof(IEnumerable<AssetPairModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAssetPairs()
        {
            var assetPairs = await _assetServiceDecorator.GetAllEnabledAssetPairsAsync();
            return Ok(assetPairs.Select(x => x.ConvertToApiModel()));
        }

        /// <summary>
        /// Get specified asset pair.
        /// </summary>
        /// <param name="id">Asset pair ID. Example: AUDUSD</param>
        /// <returns>Specified asset pair.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation("AssetPairs/{id}")]
        [ProducesResponseType(typeof(AssetPairModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAssetPair(string id)
        {
            var assetPair = await _assetServiceDecorator.GetEnabledAssetPairAsync(id);
            if (assetPair == null)
            {
                return NotFound();
            }
            return Ok(assetPair.ConvertToApiModel());
        }
    }
}
