using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Lykke.Service.Balances.Client;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class WalletsController : Controller
    {
        private readonly IBalancesClient _balancesClient;
        private readonly IAssetServiceDecorator _assetServiceDecorator;

        public WalletsController(IAssetServiceDecorator assetServiceDecorator, IBalancesClient balancesClient)
        {
            _balancesClient = balancesClient ?? throw new ArgumentNullException(nameof(balancesClient));
            _assetServiceDecorator = assetServiceDecorator ?? throw new ArgumentNullException(nameof(assetServiceDecorator));
        }

        /// <summary>
        /// Get balances.
        /// </summary>
        /// <returns>Account balances.</returns>
        [HttpGet]
        [SwaggerOperation("GetBalances")]
        [ProducesResponseType(typeof(IEnumerable<Models.ClientBalanceResponseModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBalances()
        {
            var clientId = User.GetUserId();
            var balances = await _balancesClient.GetClientBalances(clientId);
            var walletBalances = balances?.Select(Models.ClientBalanceResponseModel.Create) ?? new Models.ClientBalanceResponseModel[0].ToList();
            foreach (var wallet in walletBalances)
            {
                var asset = await _assetServiceDecorator.GetEnabledAssetAsync(wallet.AssetId);
                if (asset != null)
                {
                    wallet.Balance = wallet.Balance.TruncateDecimalPlaces(asset.Accuracy);
                    wallet.Reserved = wallet.Reserved.TruncateDecimalPlaces(asset.Accuracy);
                }
            }

            return Ok(walletBalances);
        }
    }
}
