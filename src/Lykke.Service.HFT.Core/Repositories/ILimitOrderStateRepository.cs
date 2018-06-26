using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.HFT.Contracts.Orders;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Core.Repositories
{
    public interface ILimitOrderStateRepository : IRepository<LimitOrderState>
    {
        Task<IEnumerable<LimitOrderState>> GetOrdersByStatus(string clientId, IEnumerable<OrderStatus> states, int take = 100);
    }
}