// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.Service.HFT.AutorestClient
{
    using Models;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for HighFrequencytradingAPI.
    /// </summary>
    public static partial class HighFrequencytradingAPIExtensions
    {
            /// <summary>
            /// Get all asset pairs.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static IList<AssetPairModel> GetAssetPairs(this IHighFrequencytradingAPI operations)
            {
                return operations.GetAssetPairsAsync().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get all asset pairs.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<AssetPairModel>> GetAssetPairsAsync(this IHighFrequencytradingAPI operations, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetAssetPairsWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get specified asset pair.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// Asset pair ID. Example: AUDUSD
            /// </param>
            public static AssetPairModel GetAssetPair(this IHighFrequencytradingAPI operations, string id)
            {
                return operations.GetAssetPairAsync(id).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get specified asset pair.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// Asset pair ID. Example: AUDUSD
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<AssetPairModel> GetAssetPairAsync(this IHighFrequencytradingAPI operations, string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetAssetPairWithHttpMessagesAsync(id, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get trades
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='assetId'>
            /// Asset identifier
            /// </param>
            /// <param name='assetPairId'>
            /// Asset-pair identifier
            /// </param>
            /// <param name='skip'>
            /// How many items skip before returning
            /// </param>
            /// <param name='take'>
            /// How many maximum items have to be returned
            /// </param>
            public static object GetTrades(this IHighFrequencytradingAPI operations, string apiKey, string assetId = default(string), string assetPairId = default(string), int? skip = default(int?), int? take = default(int?))
            {
                return operations.GetTradesAsync(apiKey, assetId, assetPairId, skip, take).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get trades
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='assetId'>
            /// Asset identifier
            /// </param>
            /// <param name='assetPairId'>
            /// Asset-pair identifier
            /// </param>
            /// <param name='skip'>
            /// How many items skip before returning
            /// </param>
            /// <param name='take'>
            /// How many maximum items have to be returned
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<object> GetTradesAsync(this IHighFrequencytradingAPI operations, string apiKey, string assetId = default(string), string assetPairId = default(string), int? skip = default(int?), int? take = default(int?), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetTradesWithHttpMessagesAsync(apiKey, assetId, assetPairId, skip, take, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get trade details by id
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='tradeId'>
            /// Trade identifier
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            public static HistoryTradeModel GetTrade(this IHighFrequencytradingAPI operations, string tradeId, string apiKey)
            {
                return operations.GetTradeAsync(tradeId, apiKey).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get trade details by id
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='tradeId'>
            /// Trade identifier
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<HistoryTradeModel> GetTradeAsync(this IHighFrequencytradingAPI operations, string tradeId, string apiKey, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetTradeWithHttpMessagesAsync(tradeId, apiKey, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Checks service is alive
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static object IsAlive(this IHighFrequencytradingAPI operations)
            {
                return operations.IsAliveAsync().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Checks service is alive
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<object> IsAliveAsync(this IHighFrequencytradingAPI operations, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.IsAliveWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get all order books.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static IList<OrderBook> GetOrderBooks(this IHighFrequencytradingAPI operations)
            {
                return operations.GetOrderBooksAsync().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get all order books.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<OrderBook>> GetOrderBooksAsync(this IHighFrequencytradingAPI operations, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetOrderBooksWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get order books for a specified asset pair.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='assetPairId'>
            /// Asset pair ID. Example: AUDUSD
            /// </param>
            public static IList<OrderBook> GetOrderBook(this IHighFrequencytradingAPI operations, string assetPairId)
            {
                return operations.GetOrderBookAsync(assetPairId).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get order books for a specified asset pair.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='assetPairId'>
            /// Asset pair ID. Example: AUDUSD
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<OrderBook>> GetOrderBookAsync(this IHighFrequencytradingAPI operations, string assetPairId, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetOrderBookWithHttpMessagesAsync(assetPairId, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get the last client orders.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='status'>
            /// Order status. Possible values include: 'All', 'Open', 'InOrderBook',
            /// 'Processing', 'Matched', 'Cancelled', 'Rejected'
            /// </param>
            /// <param name='take'>
            /// Default 100; max 500.
            /// </param>
            public static IList<LimitOrderState> GetOrders(this IHighFrequencytradingAPI operations, string apiKey, OrderStatus? status = default(OrderStatus?), int? take = default(int?))
            {
                return operations.GetOrdersAsync(apiKey, status, take).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get the last client orders.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='status'>
            /// Order status. Possible values include: 'All', 'Open', 'InOrderBook',
            /// 'Processing', 'Matched', 'Cancelled', 'Rejected'
            /// </param>
            /// <param name='take'>
            /// Default 100; max 500.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<LimitOrderState>> GetOrdersAsync(this IHighFrequencytradingAPI operations, string apiKey, OrderStatus? status = default(OrderStatus?), int? take = default(int?), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetOrdersWithHttpMessagesAsync(apiKey, status, take, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get the order info.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// Limit order id
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            public static LimitOrderState GetOrder(this IHighFrequencytradingAPI operations, System.Guid id, string apiKey)
            {
                return operations.GetOrderAsync(id, apiKey).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get the order info.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// Limit order id
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<LimitOrderState> GetOrderAsync(this IHighFrequencytradingAPI operations, System.Guid id, string apiKey, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetOrderWithHttpMessagesAsync(id, apiKey, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Place a market order.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='order'>
            /// </param>
            public static ResponseModelDouble PlaceMarketOrder(this IHighFrequencytradingAPI operations, string apiKey, MarketOrderRequest order = default(MarketOrderRequest))
            {
                return operations.PlaceMarketOrderAsync(apiKey, order).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Place a market order.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='order'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<ResponseModelDouble> PlaceMarketOrderAsync(this IHighFrequencytradingAPI operations, string apiKey, MarketOrderRequest order = default(MarketOrderRequest), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.PlaceMarketOrderWithHttpMessagesAsync(apiKey, order, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Place a limit order.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='order'>
            /// </param>
            public static object PlaceLimitOrder(this IHighFrequencytradingAPI operations, string apiKey, LimitOrderRequest order = default(LimitOrderRequest))
            {
                return operations.PlaceLimitOrderAsync(apiKey, order).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Place a limit order.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='order'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<object> PlaceLimitOrderAsync(this IHighFrequencytradingAPI operations, string apiKey, LimitOrderRequest order = default(LimitOrderRequest), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.PlaceLimitOrderWithHttpMessagesAsync(apiKey, order, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Cancel the limit order.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// Limit order id
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            public static ResponseModel CancelLimitOrder(this IHighFrequencytradingAPI operations, System.Guid id, string apiKey)
            {
                return operations.CancelLimitOrderAsync(id, apiKey).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Cancel the limit order.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// Limit order id
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<ResponseModel> CancelLimitOrderAsync(this IHighFrequencytradingAPI operations, System.Guid id, string apiKey, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.CancelLimitOrderWithHttpMessagesAsync(id, apiKey, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get balances.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            public static IList<ClientBalanceResponseModel> GetBalances(this IHighFrequencytradingAPI operations, string apiKey)
            {
                return operations.GetBalancesAsync(apiKey).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get balances.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiKey'>
            /// access token
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<ClientBalanceResponseModel>> GetBalancesAsync(this IHighFrequencytradingAPI operations, string apiKey, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetBalancesWithHttpMessagesAsync(apiKey, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}