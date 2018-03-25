using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common;
using Lykke.Service.Balances.Client;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models;
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
        [ProducesResponseType(typeof(IEnumerable<ClientBalanceResponseModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBalances()
        {
            var clientId = User.GetUserId();
            var balances = await _balancesClient.GetClientBalances(clientId);
            var result = new List<ClientBalanceResponseModel>();
            if (balances != null)
            {
                foreach (var wallet in balances)
                {
                    var asset = await _assetServiceDecorator.GetEnabledAssetAsync(wallet.AssetId);
                    if (asset != null)
                    {
                        result.Add(new ClientBalanceResponseModel
                        {
                            AssetId = wallet.AssetId,
                            Balance = wallet.Balance.TruncateDecimalPlaces(asset.Accuracy),
                            Reserved = wallet.Reserved.TruncateDecimalPlaces(asset.Accuracy)
                        });
                    }
                }
            }

            return Ok(result);
        }
    }
}
