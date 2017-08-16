using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.Controllers
{
	[Route("api/[controller]")]
	public class AssetPairController : Controller
	{
		private readonly CachedDataDictionary<string, IAssetPair> _assetPairs;

		public AssetPairController(CachedDataDictionary<string, IAssetPair> assetPairs)
		{
			_assetPairs = assetPairs ?? throw new ArgumentNullException(nameof(assetPairs));
		}

		/// <summary>
		/// Get all asset pairs.
		/// </summary>
		/// <returns>All asset pairs.</returns>
		[HttpGet]
		[SwaggerOperation("AssetPairs")]
		[Produces(typeof(IEnumerable<ApiAssetPairModel>))]
		public async Task<IActionResult> GetAssetPairs()
		{
			var assetPairs = await _assetPairs.Values();
			return Ok(assetPairs.Select(x => x.ConvertToApiModel()).ToArray());
		}
	}
}
