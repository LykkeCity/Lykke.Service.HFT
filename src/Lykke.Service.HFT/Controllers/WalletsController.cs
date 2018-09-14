﻿using AssetsCache.ReadModels;
using Common;
using Lykke.Service.Balances.Client;
using Lykke.Service.HFT.Contracts.Wallets;
using Lykke.Service.HFT.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Controllers
{
    /// <summary>
    /// Controller for wallet functionality.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WalletsController : Controller
    {
        private readonly IBalancesClient _balancesClient;
        private readonly IAssetsReadModel _assetsReadModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletsController"/> class.
        /// </summary>
        public WalletsController(IBalancesClient balancesClient, IAssetsReadModel assetsReadModel)
        {
            _balancesClient = balancesClient ?? throw new ArgumentNullException(nameof(balancesClient));
            _assetsReadModel = assetsReadModel;
        }

        /// <summary>
        /// Get api wallet balances.
        /// </summary>
        /// <response code="200">Api wallet balances</response>
        [HttpGet]
        [SwaggerOperation(nameof(GetBalances))]
        [ProducesResponseType(typeof(IEnumerable<BalanceModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBalances()
        {
            var clientId = User.GetUserId();
            var balances = await _balancesClient.GetClientBalances(clientId);
            var walletBalances = balances?.Select(x => new BalanceModel
            {
                AssetId = x.AssetId,
                Balance = x.Balance,
                Reserved = x.Reserved
            }) ?? Enumerable.Empty<BalanceModel>();

            foreach (var wallet in walletBalances)
            {
                var asset = _assetsReadModel.GetIfEnabled(wallet.AssetId);
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
