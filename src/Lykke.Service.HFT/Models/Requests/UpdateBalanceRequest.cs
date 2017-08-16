using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.HFT.Models.Requests
{
    public class UpdateBalanceRequest
	{
		[Required]
		public string AssetId { get; set; }
		[Required]
		public double Balance { get; set; }
	}
}
