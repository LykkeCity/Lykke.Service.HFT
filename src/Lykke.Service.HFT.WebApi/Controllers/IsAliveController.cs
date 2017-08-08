using System;
using Lykke.Service.HFT.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.WebApi.Controllers
{
	[Route("api/[controller]")]
	public class IsAliveController : Controller
	{
		/// <summary>
		/// Checks service is alive
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[SwaggerOperation("IsAlive")]
		public IsAliveResponse Get()
		{
			return new IsAliveResponse
			{
				Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion,
				Env = Environment.GetEnvironmentVariable("Env")
			};
		}
	}
}
