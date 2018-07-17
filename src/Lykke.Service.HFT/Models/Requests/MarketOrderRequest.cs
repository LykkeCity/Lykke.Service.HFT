﻿using System.ComponentModel.DataAnnotations;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Middleware;

namespace Lykke.Service.HFT.Models.Requests
{
    public class MarketOrderRequest
    {
        [Required]
        public string AssetPairId { get; set; }
        [Required]
        public string Asset { get; set; }
        [Required]
        public OrderAction OrderAction { get; set; }
        [Required, DoubleGreaterThanZero]
        public double Volume { get; set; }
    }
}