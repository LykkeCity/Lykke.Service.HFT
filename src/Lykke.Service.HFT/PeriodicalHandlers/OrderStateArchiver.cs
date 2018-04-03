using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;

namespace Lykke.Service.HFT.PeriodicalHandlers
{
    public class OrderStateArchiver : TimerPeriod
    {
        private readonly ILog _log;
        private readonly IRepository<LimitOrderState> _orderStateRepository;
        private readonly ILimitOrderStateRepository _orderStateArchive;

        public OrderStateArchiver(
            TimeSpan checkInterval,
            ILog log,
            IRepository<LimitOrderState> orderStateRepository,
            ILimitOrderStateRepository orderStateArchive)
            : base(nameof(GhostOrdersRemover), (int)checkInterval.TotalMilliseconds, log)
        {
            _log = log.CreateComponentScope(nameof(GhostOrdersRemover));
            _orderStateRepository = orderStateRepository;
            _orderStateArchive = orderStateArchive;
        }

        public override async Task Execute()
        {

            _log.WriteInfo("OrderStateArchiver", null, "Starting archiving");

            Expression<Func<LimitOrderState, bool>> filter = x =>
                x.Status != OrderStatus.InOrderBook && x.Status != OrderStatus.Processing && x.Status != OrderStatus.Pending
                && (x.LastMatchTime == null && x.LastMatchTime < DateTime.UtcNow.AddDays(14) || x.CreatedAt < DateTime.UtcNow.AddDays(14));

            var chunkSize = 1000;
            while (true)
            {
                var notActiveOrders = _orderStateRepository.All()
                    .Where(filter)
                    .Take(chunkSize)
                    .ToList();
                if (notActiveOrders.Count == 0)
                    break;

                _log.WriteInfo("OrderStateArchiver", null, $"Next {notActiveOrders.Count} orders");
                await _orderStateArchive.AddAsync(notActiveOrders);
            }

            await _orderStateRepository.DeleteAsync(filter);
        }
    }
}
