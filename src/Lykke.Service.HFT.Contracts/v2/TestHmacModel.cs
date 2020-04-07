using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.HFT.Contracts.v2
{
    /// <summary>
    /// Model to test hmac authentication
    /// </summary>
    public class TestHmacModel
    {
        [Required]
        public string Symbol { get; set; }
        [Required]
        public string Side { get; set; }
        [Required]
        public string Type { get; set; }

    }
}
