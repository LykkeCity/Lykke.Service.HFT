using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Services.Assets;
using Lykke.Service.HFT.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.Controllers
{
	[Route("api/[controller]")]
	public class AssetPairsController : Controller
	{
		private readonly IAssetPairsManager _assetPairsManager;

		public AssetPairsController(IAssetPairsManager assetPairsManager)
		{
			_assetPairsManager = assetPairsManager ?? throw new ArgumentNullException(nameof(assetPairsManager));
		}

		/// <summary>
		/// Get all asset pairs.
		/// </summary>
		/// <returns>All asset pairs.</returns>
		[HttpGet]
		[SwaggerOperation("AssetPairs")]
		[ProducesResponseType(typeof(IEnumerable<ApiAssetPairModel>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetAssetPairs()
		{
			var assetPairs = await _assetPairsManager.GetAllEnabledAssetPairsAsync();
			return Ok(assetPairs.Select(x => x.ConvertToApiModel()).ToArray());
		}

		/// <summary>
		/// Get specified asset pair.
		/// </summary>
		/// <param name="id">Asset pair ID. Example: AUDUSD</param>
		/// <returns>Specified asset pair.</returns>
		[HttpGet("{id}")]
		[SwaggerOperation("AssetPairs/{id}")]
		[ProducesResponseType(typeof(ApiAssetPairModel), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> GetAssetPair(string id)
		{
			var assetPair = await _assetPairsManager.TryGetEnabledAssetPairAsync(id);
			if (assetPair == null)
			{
				return NotFound();
			}
			return Ok(assetPair.ConvertToApiModel());
		}
	}
}
