using System.ComponentModel.DataAnnotations;
using Lykke.Service.HFT.Middleware;

namespace Lykke.Service.HFT.Models.Requests
{
    public class CashOutRequest
	{
		[Required]
		public string AssetId { get; set; }
		[Required, NonZero]
		public double Amount { get; set; }
	}
}
