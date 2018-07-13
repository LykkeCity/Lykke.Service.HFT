using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.HFT.Contracts.Orders;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using MoreLinq;

namespace Lykke.Service.HFT.PeriodicalHandlers
{
    internal class OrderStateArchiver : TimerPeriod
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
            ILogFactory logFactory,
            IRepository<LimitOrderState> orderStateCache,
            ILimitOrderStateArchive orderStateArchive)
            : base(checkInterval, logFactory)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            _log = logFactory.CreateLog(nameof(OrderStateArchiver));
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
                    _log.Warning($"Too small chunk size {chunkSize} ");
                    break;
                }

                sw.Restart();
                _log.Info($"1. Getting orders (max {chunkSize}).");
                try
                {
                    var notActiveOrders = (await _orderStateCache.FilterAsync(filter, chunkSize)).ToList();
                    if (notActiveOrders.Count == 0)
                    {
                        _log.Info("Finished");
                        break;
                    }

                    _log.Info($"2. Got {notActiveOrders.Count} orders in {sw.Elapsed.TotalSeconds} sec.");
                    sw.Restart();

                    notActiveOrders.Where(x => x.CreatedAt < MinimalTime).ForEach(x => x.CreatedAt = MinimalTime);
                    notActiveOrders.Where(x => x.Registered < MinimalTime).ForEach(x => x.Registered = MinimalTime);
                    await _orderStateArchive.AddAsync(notActiveOrders);
                    _log.Info($"3. Migrated to azure in {sw.Elapsed.TotalMinutes} min.");
                    sw.Restart();

                    await _orderStateCache.DeleteAsync(notActiveOrders);
                    _log.Info($"4. Deleted from mongo in {sw.Elapsed.TotalMinutes} min.");
                }
                catch (Exception exception)
                {
                    _log.Warning("Error while archiving limit orders.", exception);
                    chunkSize = (int)(chunkSize * 0.8);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}
