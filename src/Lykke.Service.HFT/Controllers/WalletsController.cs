using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Accounts;
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

		public WalletsController(IWalletsRepository walletsRepository)
		{
			_walletsRepository = walletsRepository ?? throw new ArgumentNullException(nameof(walletsRepository));
		}

		/// <summary>
		/// Get balance.
		/// </summary>
		/// <returns>Client balance.</returns>
		[HttpGet]
		[SwaggerOperation("Wallets")]
		[Produces(typeof(IEnumerable<IWallet>))]
		public async Task<IActionResult> GetWallets()
		{
			var clientId = User.GetUserId();
			var wallets = await _walletsRepository.GetAsync(clientId);
			
			return Ok(wallets);
		}
	}
}
