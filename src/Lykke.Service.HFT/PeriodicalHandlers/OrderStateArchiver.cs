using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using MoreLinq;

namespace Lykke.Service.HFT.PeriodicalHandlers
{
    public class OrderStateArchiver : TimerPeriod
    {
        private const int DefaultChunkSize = 5000;
        private const int MinimalChunkSize = 100;
        private static readonly DateTime MinimalTime = new DateTime(1601, 1, 1);
        private readonly ILog _log;
        private readonly TimeSpan _activeOrdersWindow;
        private readonly IRepository<LimitOrderState> _orderStateCache;
        private readonly ILimitOrderStateArchive _orderStateArchive;

        public OrderStateArchiver(
            TimeSpan checkInterval,
            TimeSpan activeOrdersWindow,
            ILog log,
            IRepository<LimitOrderState> orderStateCache,
            ILimitOrderStateArchive orderStateArchive)
            : base(nameof(OrderStateArchiver), (int)checkInterval.TotalMilliseconds, log)
        {
            _log = log.CreateComponentScope(nameof(OrderStateArchiver));
            _activeOrdersWindow = activeOrdersWindow;
            _orderStateCache = orderStateCache ?? throw new ArgumentNullException(nameof(orderStateCache));
            _orderStateArchive = orderStateArchive ?? throw new ArgumentNullException(nameof(orderStateArchive));
        }

        public override async Task Execute()
        {
            var minimalDate = DateTime.UtcNow.Add(-_activeOrdersWindow);
            Expression<Func<LimitOrderState, bool>> filter = x =>
                x.Status != OrderStatus.InOrderBook && x.Status != OrderStatus.Processing && x.Status != OrderStatus.Pending
                && (x.LastMatchTime == null && x.CreatedAt < minimalDate || x.LastMatchTime < minimalDate);

            var chunkSize = DefaultChunkSize;
            var sw = new Stopwatch();
            while (true)
            {
                if (chunkSize < MinimalChunkSize)
                {
                    _log.WriteWarning("OrderStateArchiver", null, $"Too small chunk size {chunkSize} ");
                    break;
                }

                sw.Restart();
                _log.WriteInfo("OrderStateArchiver", null, $"1. Getting orders (max {chunkSize}).");
                try
                {
                    var notActiveOrders = (await _orderStateCache.FilterAsync(filter, chunkSize)).ToList();
                    if (notActiveOrders.Count == 0)
                    {
                        _log.WriteInfo("OrderStateArchiver", null, "Finished");
                        break;
                    }

                    _log.WriteInfo("OrderStateArchiver", null, $"2. Got {notActiveOrders.Count} orders in {sw.Elapsed.TotalSeconds} sec.");
                    sw.Restart();

                    notActiveOrders.Where(x => x.CreatedAt < MinimalTime).ForEach(x => x.CreatedAt = MinimalTime);
                    await _orderStateArchive.AddAsync(notActiveOrders);
                    _log.WriteInfo("OrderStateArchiver", null, $"3. Migrated to azure in {sw.Elapsed.TotalMinutes} min.");
                    sw.Restart();

                    await _orderStateCache.DeleteAsync(notActiveOrders);
                    _log.WriteInfo("OrderStateArchiver", null, $"4. Deleted from mongo in {sw.Elapsed.TotalMinutes} min.");
                }
                catch (Exception exception)
                {
                    _log.WriteWarning("OrderStateArchiver", null, "", exception);
                    chunkSize = (int)(chunkSize * 0.8);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}
