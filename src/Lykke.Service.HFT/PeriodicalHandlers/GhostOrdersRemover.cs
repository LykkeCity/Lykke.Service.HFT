﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.HFT.Contracts.Orders;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;

namespace Lykke.Service.HFT.PeriodicalHandlers
{
    internal class GhostOrdersRemover : TimerPeriod
    {
        private const int DefaultChunkSize = 5000;
        private readonly IOrderBooksService _orderBooksService;
        private readonly ILog _log;
        private readonly IRepository<LimitOrderState> _orderStateRepository;

        public GhostOrdersRemover(
            TimeSpan checkInterval,
            IOrderBooksService orderBooksService,
            ILogFactory logFactory,
            IRepository<LimitOrderState> orderStateRepository)
            : base(checkInterval, logFactory)
        {
            _orderBooksService = orderBooksService;
            _log = logFactory.CreateLog(this);
            _orderStateRepository = orderStateRepository;
        }

        public override async Task Execute()
        {
            var minimalDate = DateTime.UtcNow.AddHours(-1);
            Expression<Func<LimitOrderState, bool>> filter = x =>
                x.Status == OrderStatus.InOrderBook || x.Status == OrderStatus.Processing || x.Status == OrderStatus.Pending
                && (x.LastMatchTime == null && x.CreatedAt < minimalDate || x.LastMatchTime < minimalDate);
            var ordersInOrderBookState = (await _orderStateRepository.FilterAsync(filter, 
                batchSize: DefaultChunkSize,
                limit: null)).ToList();

            var assetPairs = ordersInOrderBookState.Select(x => x.AssetPairId).Distinct();
            var orderBook = await _orderBooksService.GetOrderIdsAsync(assetPairs);

            var ordersToCancel = ordersInOrderBookState.Where(order => !orderBook.Contains(order.Id)).ToList();
            if (ordersToCancel.Count > 0)
            {
                _log.Warning($"{ordersToCancel.Count} orders are not in orderbook. Setting status as canceled.", context: ordersToCancel);
                foreach (var order in ordersToCancel)
                {
                    order.Status = OrderStatus.Cancelled;
                    await _orderStateRepository.Update(order);
                }
            }
        }
    }
}
