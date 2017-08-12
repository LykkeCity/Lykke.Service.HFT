using System;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.HFT.WebApi.Controllers
{
	[Route("api/[controller]")]
	public class ApiKeysController : Controller
	{
		/// <summary>
		/// Generate API key.
		/// </summary>
		/// <returns></returns>
		[HttpPost("GenerateKey")]
		public string GenerateKey()
		{
			return Guid.NewGuid().ToString();
		}
	}
}
