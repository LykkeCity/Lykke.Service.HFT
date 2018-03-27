using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;

namespace Lykke.Service.HFT.PeriodicalHandlers
{
    public class GhostOrdersRemover : TimerPeriod
    {
        private readonly IOrderBooksService _orderBooksService;
        private readonly ILog _log;
        private readonly IRepository<LimitOrderState> _orderStateRepository;

        public GhostOrdersRemover(
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
            var ordersInOrderBookState = _orderStateRepository.All()
                .Where(x => x.Status == OrderStatus.InOrderBook || x.Status == OrderStatus.Processing).ToList();
            var assetPairs = ordersInOrderBookState.Select(x => x.AssetPairId).Distinct();
            var orderBook = await _orderBooksService.GetOrderIdsAsync(assetPairs);

            var ordersToCancel = ordersInOrderBookState.Where(order => !orderBook.Contains(order.Id)).ToList();
            if (ordersToCancel.Count > 0)
            {
                _log.WriteWarning("Ghost orders check", ordersToCancel, $"{ordersToCancel.Count} orders are not in orderbook. Setting status as canceled.");
                foreach (var order in ordersToCancel)
                {
                    order.Status = OrderStatus.Cancelled;
                    await _orderStateRepository.Update(order);
                }
            }
        }
    }
}
