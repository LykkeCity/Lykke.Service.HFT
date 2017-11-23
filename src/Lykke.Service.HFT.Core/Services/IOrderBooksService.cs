using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IOrderBooksService
    {
        Task<IEnumerable<OrderBook>> GetAllAsync();
        Task<IEnumerable<OrderBook>> GetAsync(string assetPairId);
        Task<double?> GetBestPrice(string assetPair, bool buy);
    }
}
