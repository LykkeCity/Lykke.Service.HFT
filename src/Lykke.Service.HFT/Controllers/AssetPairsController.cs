using System;
using System.Collections.Generic;
using System.Linq;
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
		[Produces(typeof(IEnumerable<ApiAssetPairModel>))]
		public async Task<IActionResult> GetAssetPairs()
		{
			var assetPairs = await _assetPairsManager.GetAllEnabledAsync();
			return Ok(assetPairs.Select(x => x.ConvertToApiModel()).ToArray());
		}
	}
}
