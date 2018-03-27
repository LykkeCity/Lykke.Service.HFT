using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IOrderBooksService
    {
        Task<ICollection<Guid>> GetOrderIdsAsync(IEnumerable<string> assetPairs);
        Task<IEnumerable<OrderBook>> GetAllAsync();
        Task<IEnumerable<OrderBook>> GetAsync(string assetPairId);
    }
}
