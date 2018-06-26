using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.HFT.Contracts.Orders;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;

namespace Lykke.Service.HFT.PeriodicalHandlers
{
    public class PendingOrdersChecker : TimerPeriod
    {
        private const int DefaultChunkSize = 5000;
        private readonly IOrderBooksService _orderBooksService;
        private readonly ILog _log;
        private readonly IRepository<LimitOrderState> _orderStateRepository;

        public PendingOrdersChecker(
            TimeSpan checkInterval,
            IOrderBooksService orderBooksService,
            ILog log,
            IRepository<LimitOrderState> orderStateRepository)
            : base(nameof(GhostOrdersRemover), (int)checkInterval.TotalMilliseconds, log)
        {
            _orderBooksService = orderBooksService;
            _log = log.CreateComponentScope(nameof(GhostOrdersRemover));
            _orderStateRepository = orderStateRepository;
        }

        public override async Task Execute()
        {
            Expression<Func<LimitOrderState, bool>> filter = x => x.Status == OrderStatus.Pending;
            var pendingOrders = (await _orderStateRepository.FilterAsync(filter, DefaultChunkSize)).ToList();

            var assetPairs = pendingOrders.Select(x => x.AssetPairId).Distinct();
            var orderBook = await _orderBooksService.GetOrderIdsAsync(assetPairs);

            var ordersInOrderBook = pendingOrders.Where(order => orderBook.Contains(order.Id)).ToList();
            if (ordersInOrderBook.Count > 0)
            {
                _log.WriteWarning("Pending orders check", ordersInOrderBook, $"{ordersInOrderBook.Count} orders are in orderbook.");
                foreach (var order in ordersInOrderBook)
                {
                    order.Status = OrderStatus.InOrderBook;
                    await _orderStateRepository.Update(order);
                }
            }
        }
    }
}
