using System;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	public class BalanceController : Controller
	{
		private readonly IMatchingEngineAdapter _matchingEngineAdapter;

		public BalanceController(IMatchingEngineAdapter frequencyTradingService)
		{
			_matchingEngineAdapter = frequencyTradingService ?? throw new ArgumentNullException(nameof(frequencyTradingService));
		}
		

		/// <summary>
		/// Cash in / out.
		/// </summary>
		[HttpPost("CashOut")]
		[SwaggerOperation("CashOut")]
		public async Task<IActionResult> CashOut([FromBody] CashOutRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var clientId = User.GetUserId();
			await _matchingEngineAdapter.CashInOutAsync(
				clientId: clientId,
				assetId: request.AssetId,
				amount: request.Amount
				);
			return Ok();
		}

		/// <summary>
		/// Update balance.
		/// </summary>
		[HttpPost("Update")]
		[SwaggerOperation("Update")]
		public async Task<IActionResult> UpdateBalance([FromBody] UpdateBalanceRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var clientId = User.GetUserId();
			await _matchingEngineAdapter.UpdateBalanceAsync(
				clientId: clientId,
				assetId: request.AssetId,
				value: request.Balance
				);
			return Ok();
		}
		
	}
}
