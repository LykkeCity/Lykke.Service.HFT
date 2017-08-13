using Microsoft.AspNetCore.Builder;

namespace Lykke.Service.HFT.Middleware
{
	public class KeyAuthOptions : AuthenticationOptions
	{
		public const string DefaultHeaderName = "api-key";
		public string KeyHeaderName { get; set; } = DefaultHeaderName;

		public KeyAuthOptions()
		{
			AuthenticationScheme = "Automatic";
		}
	}
}
