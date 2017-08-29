using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Lykke.Service.HFT.Core.Accounts;
using Lykke.Service.HFT.Core.Services.Assets;
using Lykke.Service.HFT.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class WalletsController : Controller
    {
        private readonly IWalletsRepository _walletsRepository;
        private readonly IAssetPairsManager _assetPairsManager;

        public WalletsController(IWalletsRepository walletsRepository, IAssetPairsManager assetPairsManager)
        {
            _walletsRepository = walletsRepository ?? throw new ArgumentNullException(nameof(walletsRepository));
            _assetPairsManager = assetPairsManager ?? throw new ArgumentNullException(nameof(assetPairsManager));
        }

        /// <summary>
        /// Get balance.
        /// </summary>
        /// <returns>Client balance.</returns>
        [HttpGet]
        [SwaggerOperation("Wallets")]
        [ProducesResponseType(typeof(IEnumerable<Wallet>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetWallets()
        {
            var clientId = User.GetUserId();
            var wallets = (await _walletsRepository.GetAsync(clientId)).Select(x => new Wallet { Balance = x.Balance, AssetId = x.AssetId, Reserved = x.Reserved }).ToList();
            foreach (var wallet in wallets)
            {
                var asset = await _assetPairsManager.TryGetEnabledAssetAsync(wallet.AssetId);
                if (asset != null)
                {
                    wallet.Balance = wallet.Balance.TruncateDecimalPlaces(asset.Accuracy);
                    wallet.Reserved = wallet.Reserved.TruncateDecimalPlaces(asset.Accuracy);
                }
            }

            return Ok(wallets);
        }
    }
}
