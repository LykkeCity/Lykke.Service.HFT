using JetBrains.Annotations;
using Lykke.Service.Assets.Client.Models.v3;
using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Contracts.Orders;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.History.Client;
using Lykke.Service.History.Contracts.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using OrderStatus = Lykke.Service.HFT.Contracts.Orders.OrderStatus;
using OrderType = Lykke.Service.HFT.Contracts.Orders.OrderType;
using Swashbuckle.AspNetCore.Annotations;

namespace Lykke.Service.HFT.Controllers
{
    /// <summary>
    /// Controller for order functionality.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : Controller
    {
        private const int MaxPageSize = 500;
        private readonly RequestValidator _requestValidator;
        private readonly IMatchingEngineAdapter _matchingEngineAdapter;
        private readonly IAssetPairsReadModelRepository _assetPairsReadModel;
        private readonly IAssetsReadModelRepository _assetsReadModel;
        private readonly IHistoryClient _historyClient;
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersController"/> class.
        /// </summary>
        public OrdersController(
            IMatchingEngineAdapter frequencyTradingService,
            RequestValidator requestValidator,
            IAssetPairsReadModelRepository assetPairsReadModel,
            IAssetsReadModelRepository assetsReadModel,
            [NotNull] IHistoryClient historyClient,
            ILogFactory logFactory)
        {
            _matchingEngineAdapter = frequencyTradingService ?? throw new ArgumentNullException(nameof(frequencyTradingService));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _assetPairsReadModel = assetPairsReadModel;
            _assetsReadModel = assetsReadModel;
            _historyClient = historyClient ?? throw new ArgumentNullException(nameof(historyClient));
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Get the last client orders.
        /// </summary>
        /// <param name="status">Order status</param>
        /// <param name="take">The amount of orders to take, default 100; max 500.</param>
        /// <param name="orderType">The type of orders.</param>
        /// <response code="200">The latest client orders.</response>
        [HttpGet]
        [SwaggerOperation(nameof(GetOrders))]
        [ProducesResponseType(typeof(IEnumerable<LimitOrderStateModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOrders(OrderStatusQuery? status = null, int? take = 100, OrderType orderType = OrderType.Unknown)
        {
            var toTake = take.ValidateAndGetValue(nameof(take), MaxPageSize, 100);
            if (toTake.Error != null)
            {
                return BadRequest(toTake.Error);
            }

            if (!status.HasValue)
            {
                status = OrderStatusQuery.All;
            }

            var walletId = Guid.Parse(User.GetUserId());
            var orderTypes = orderType == OrderType.Unknown
                ? new History.Contracts.Enums.OrderType[0]
                : new[] { (History.Contracts.Enums.OrderType)orderType };

            IEnumerable<OrderModel> orders = new List<OrderModel>();

            try
            {
                switch (status)
                {
                    case OrderStatusQuery.All:
                        orders = await _historyClient.OrdersApi.GetOrdersByWalletAsync(
                            walletId,
                            new History.Contracts.Enums.OrderStatus[0],
                            orderTypes,
                            0,
                            toTake.Result);
                        break;
                    case OrderStatusQuery.Open:
                        orders = await _historyClient.OrdersApi.GetActiveOrdersByWalletAsync(walletId, 0,
                            toTake.Result);
                        break;
                    case OrderStatusQuery.InOrderBook:
                        orders = await _historyClient.OrdersApi.GetOrdersByWalletAsync(
                            walletId,
                            new[] {History.Contracts.Enums.OrderStatus.Placed},
                            orderTypes,
                            0,
                            toTake.Result);
                        break;
                    case OrderStatusQuery.Processing:
                        orders = await _historyClient.OrdersApi.GetOrdersByWalletAsync(
                            walletId,
                            new[] {History.Contracts.Enums.OrderStatus.PartiallyMatched},
                            orderTypes,
                            0,
                            toTake.Result);
                        break;
                    case OrderStatusQuery.Matched:
                        orders = await _historyClient.OrdersApi.GetOrdersByWalletAsync(
                            walletId,
                            new[] {History.Contracts.Enums.OrderStatus.Matched},
                            orderTypes,
                            0,
                            toTake.Result);
                        break;
                    case OrderStatusQuery.Replaced:
                        orders = await _historyClient.OrdersApi.GetOrdersByWalletAsync(
                            walletId,
                            new[] {History.Contracts.Enums.OrderStatus.Replaced},
                            orderTypes,
                            0,
                            toTake.Result);
                        break;
                    case OrderStatusQuery.Cancelled:
                        orders = await _historyClient.OrdersApi.GetOrdersByWalletAsync(
                            walletId,
                            new[] {History.Contracts.Enums.OrderStatus.Cancelled},
                            orderTypes,
                            0,
                            toTake.Result);
                        break;
                    case OrderStatusQuery.Rejected:
                        orders = await _historyClient.OrdersApi.GetOrdersByWalletAsync(
                            walletId,
                            new[] {History.Contracts.Enums.OrderStatus.Rejected},
                            orderTypes,
                            0,
                            toTake.Result);
                        break;
                    default:
                        return BadRequest(
                            ResponseModel.CreateInvalidFieldError("status", $"Invalid status: <{status}>"));
                }
            }
            catch (Exception ex)
            {
                _log.Warning("Error getting orders", ex, context: new {walletId = walletId, status, orderType, take = toTake.Result}.ToJson());
            }

            return Ok(orders.Select(ToModel));
        }


        /// <summary>
        /// Get the order details.
        /// </summary>
        /// <param name="id">Limit order id</param>
        /// <response code="200">The order details.</response>
        /// <response code="404">Order could not be found</response>
        [HttpGet("{id}")]
        [SwaggerOperation(nameof(GetOrder))]
        [ProducesResponseType(typeof(LimitOrderStateModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var order = await _historyClient.OrdersApi.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(ToModel(order));
        }

        /// <summary>
        /// Place a new market order.
        /// </summary>
        /// <response code="200">The average strike price for the settled market order.</response>
        /// <response code="404">Requested asset pair could not be found or is disabled.</response>
        [HttpPost("market")]
        [Obsolete("Use the v2 version for placing market orders")]
        [SwaggerOperation(nameof(PlaceMarketOrder))]
        [ProducesResponseType(typeof(ResponseModel<double>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceMarketOrderOld(PlaceMarketOrderModel order)
        {
            var result = await PlaceMarketOrder(order);

            if (!(result is OkObjectResult okResult))
            {
                return result;
            }

            var response = (MarketOrderResponseModel)okResult.Value;
            return Ok(response.Price);
        }

        /// <summary>
        /// Place a new market order.
        /// </summary>
        /// <response code="200">The placed market order results.</response>
        /// <response code="404">Requested asset pair could not be found or is disabled.</response>
        [HttpPost("v2/market")]
        [SwaggerOperation(nameof(PlaceMarketOrder))]
        [ProducesResponseType(typeof(MarketOrderResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceMarketOrder(PlaceMarketOrderModel order)
        {
            var assetPair = _assetPairsReadModel.TryGetIfEnabled(order.AssetPairId);
            if (assetPair == null)
            {
                return NotFound($"Asset-pair {order.AssetPairId} could not be found or is disabled.");
            }

            if (!_requestValidator.ValidateAssetPair(order.AssetPairId, assetPair, out var badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var baseAsset = _assetsReadModel.TryGetIfEnabled(assetPair.BaseAssetId);
            var quotingAsset = _assetsReadModel.TryGetIfEnabled(assetPair.QuotingAssetId);
            if (!_requestValidator.ValidateAsset(assetPair, order.Asset, baseAsset, quotingAsset, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var straight = order.Asset == baseAsset.Id || order.Asset == baseAsset.DisplayId;
            var asset = straight ? baseAsset : quotingAsset;
            var volume = order.Volume;
            var minVolume = straight ? assetPair.MinVolume : assetPair.MinInvertedVolume;
            if (!_requestValidator.ValidateVolume(volume, minVolume, asset.DisplayId, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var walletId = User.GetUserId();
            var response = await _matchingEngineAdapter.PlaceMarketOrderAsync(
                clientId: walletId,
                assetPair: assetPair,
                orderAction: order.OrderAction,
                volume: volume,
                straight: straight,
                reservedLimitVolume: null);

            if (response.Error != null)
            {
                return BadRequest(response);
            }

            return Ok(response.Result);
        }

        /// <summary>
        /// Place a new limit order.
        /// </summary>
        /// <response code="200">The id of the placed limit order.</response>
        /// <response code="404">Requested asset pair could not be found or is disabled.</response>
        [HttpPost("limit")]
        [Obsolete("Use the v2 version for placing limit orders")]
        [SwaggerOperation(nameof(PlaceLimitOrder))]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceLimitOrderOld(PlaceLimitOrderModel order)
        {
            var result = await PlaceLimitOrder(order);

            if (!(result is OkObjectResult okResult))
            {
                return result;
            }

            var response = (LimitOrderResponseModel)okResult.Value;
            return Ok(response.Id);
        }

        /// <summary>
        /// Place a new limit order.
        /// </summary>
        /// <response code="200">The placed limit order results.</response>
        /// <response code="404">Requested asset pair could not be found or is disabled.</response>
        [HttpPost("v2/limit")]
        [SwaggerOperation(nameof(PlaceLimitOrder))]
        [ProducesResponseType(typeof(LimitOrderResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceLimitOrder(PlaceLimitOrderModel order)
        {
            var assetPair = _assetPairsReadModel.TryGetIfEnabled(order.AssetPairId);
            if (assetPair == null)
            {
                return NotFound($"Asset-pair {order.AssetPairId} could not be found or is disabled.");
            }

            if (!_requestValidator.ValidateAssetPair(order.AssetPairId, assetPair, out var badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var asset = _assetsReadModel.TryGetIfEnabled(assetPair.BaseAssetId);
            if (asset == null)
                throw new InvalidOperationException($"Base asset '{assetPair.BaseAssetId}' for asset pair '{assetPair.Id}' not found.");

            var price = order.Price;
            if (!_requestValidator.ValidatePrice(price, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var volume = order.Volume;
            var minVolume = assetPair.MinVolume;
            if (!_requestValidator.ValidateVolume(volume, minVolume, asset.DisplayId, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var walletId = User.GetUserId();
            var response = await _matchingEngineAdapter.PlaceLimitOrderAsync(
                clientId: walletId,
                assetPair: assetPair,
                orderAction: order.OrderAction,
                volume: volume,
                price: price);
            if (response.Error != null)
            {
                return BadRequest(response);
            }

            return Ok(response.Result);
        }

        /// <summary>
        /// Place a new limit order.
        /// </summary>
        /// <response code="200">The placed limit order results.</response>
        /// <response code="404">Requested asset pair could not be found or is disabled.</response>
        [HttpPost("stoplimit")]
        [SwaggerOperation(nameof(PlaceLimitOrder))]
        [ProducesResponseType(typeof(LimitOrderResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceStopLimitOrder([FromBody] PlaceStopLimitOrderModel order)
        {
            var assetPair = _assetPairsReadModel.TryGetIfEnabled(order.AssetPairId);
            if (assetPair == null)
            {
                return NotFound($"Asset-pair {order.AssetPairId} could not be found or is disabled.");
            }

            if (!_requestValidator.ValidateAssetPair(order.AssetPairId, assetPair, out var badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var asset = _assetsReadModel.TryGetIfEnabled(assetPair.BaseAssetId);
            if (asset == null)
                throw new InvalidOperationException($"Base asset '{assetPair.BaseAssetId}' for asset pair '{assetPair.Id}' not found.");


            var lowerPrice = order.LowerPrice;
            if (lowerPrice.HasValue && !_requestValidator.ValidatePrice(lowerPrice.Value, out badRequestModel, nameof(PlaceStopLimitOrderModel.LowerPrice)))
            {
                return BadRequest(badRequestModel);
            }

            var lowerLimitPrice = order.LowerLimitPrice;
            if (lowerLimitPrice.HasValue && !_requestValidator.ValidatePrice(lowerLimitPrice.Value, out badRequestModel, nameof(PlaceStopLimitOrderModel.LowerLimitPrice)))
            {
                return BadRequest(badRequestModel);
            }

            if ((lowerPrice.HasValue && !lowerLimitPrice.HasValue) ||
                (!lowerPrice.HasValue && lowerLimitPrice.HasValue))
            {
                return BadRequest(ResponseModel.CreateInvalidFieldError(nameof(order.LowerPrice), "When lower price is send then also lower limit price is required and vice versa."));
            }

            var upperPrice = order.UpperPrice;
            if (upperPrice.HasValue && !_requestValidator.ValidatePrice(upperPrice.Value, out badRequestModel, nameof(PlaceStopLimitOrderModel.UpperPrice)))
            {
                return BadRequest(badRequestModel);
            }

            var upperLimitPrice = order.UpperLimitPrice;
            if (upperLimitPrice.HasValue && !_requestValidator.ValidatePrice(upperLimitPrice.Value, out badRequestModel, nameof(PlaceStopLimitOrderModel.UpperLimitPrice)))
            {
                return BadRequest(badRequestModel);
            }

            if ((upperPrice.HasValue && !upperLimitPrice.HasValue) ||
                (!upperPrice.HasValue && upperLimitPrice.HasValue))
            {
                return BadRequest(ResponseModel.CreateInvalidFieldError(nameof(order.UpperPrice), "When upper price is send then also upper limit price is required and vice versa."));
            }

            if (new[] { lowerPrice, lowerLimitPrice, upperPrice, upperLimitPrice }.All(x => !x.HasValue))
            {
                return BadRequest(ResponseModel.CreateFail(ErrorCodeType.Rejected,
                    "At least lower or upper prices are needed for a stop order."));
            }

            var volume = order.Volume;
            var minVolume = assetPair.MinVolume;
            if (!_requestValidator.ValidateVolume(volume, minVolume, asset.DisplayId, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var walletId = User.GetUserId();
            var response = await _matchingEngineAdapter.PlaceStopLimitOrderAsync(
                clientId: walletId,
                assetPair: assetPair,
                orderAction: order.OrderAction,
                volume: volume,
                lowerPrice: lowerPrice,
                lowerLimitPrice: lowerLimitPrice,
                upperPrice: upperPrice,
                upperLimitPrice: upperLimitPrice);
            if (response.Error != null)
            {
                return BadRequest(response);
            }

            return Ok(response.Result);
        }

        /// <summary>
        /// Place a bulk limit order.
        /// </summary>
        /// <response code="200">The placed bulk order results.</response>
        /// <response code="404">Requested asset pair could not be found or is disabled.</response>
        [HttpPost("bulk")]
        [SwaggerOperation(nameof(PlaceBulkOrder))]
        [ProducesResponseType(typeof(BulkOrderResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceBulkOrder([FromBody] PlaceBulkOrderModel order)
        {
            var assetPair = _assetPairsReadModel.TryGetIfEnabled(order.AssetPairId);
            if (assetPair == null)
            {
                return NotFound($"Asset-pair {order.AssetPairId} could not be found or is disabled.");
            }

            if (!_requestValidator.ValidateAssetPair(order.AssetPairId, assetPair, out var badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var asset = _assetsReadModel.TryGetIfEnabled(assetPair.BaseAssetId);
            if (asset == null)
                throw new InvalidOperationException($"Base asset '{assetPair.BaseAssetId}' for asset pair '{assetPair.Id}' not found.");

            var items = order.Orders?.ToArray() ?? new BulkOrderItemModel[0];
            foreach (var item in items)
            {
                var price = item.Price;
                if (!_requestValidator.ValidatePrice(price, out badRequestModel))
                {
                    return BadRequest(badRequestModel);
                }

                var volume = item.Volume;
                var minVolume = assetPair.MinVolume;
                if (!_requestValidator.ValidateVolume(volume, minVolume, asset.DisplayId, out badRequestModel))
                {
                    return BadRequest(badRequestModel);
                }

                item.Price = price;
                item.Volume = volume;
            }

            var walletId = User.GetUserId();
            var response = await _matchingEngineAdapter.PlaceBulkLimitOrderAsync(
                clientId: walletId,
                assetPair: assetPair,
                items: items,
                cancelPrevious: order.CancelPreviousOrders,
                cancelMode: order.CancelMode
                );
            if (response.Error != null)
            {
                return BadRequest(response);
            }

            return Ok(response.Result);
        }

        /// <summary>
        /// Cancel a limit order.
        /// </summary>
        /// <param name="id">Limit order id</param>
        /// <response code="200">Limit order has been canceled.</response>
        /// <response code="403">You don't have permission to cancel that limit order.</response>
        /// <response code="404">Limit order could not be found.</response>
        [HttpPost("{id}/Cancel")]
        [SwaggerOperation(nameof(CancelLimitOrderOld))]
        [Obsolete("Use HTTP delete {id} instead.")]
        public Task<IActionResult> CancelLimitOrderOld(Guid id)
            => CancelLimitOrder(id);

        /// <summary>
        /// Cancel a limit order.
        /// </summary>
        /// <param name="id">Limit order id</param>
        /// <response code="200">Limit order has been canceled.</response>
        /// <response code="403">You don't have permission to cancel that limit order.</response>
        /// <response code="404">Limit order could not be found.</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(nameof(CancelLimitOrder))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> CancelLimitOrder(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var walletId = Guid.Parse(User.GetUserId());

            var order = await _historyClient.OrdersApi.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            if (order.WalletId != walletId)
            {
                return Forbid();
            }

            if (order.Status == History.Contracts.Enums.OrderStatus.Cancelled
                || order.Status == History.Contracts.Enums.OrderStatus.Replaced
                || order.Status == History.Contracts.Enums.OrderStatus.Rejected)
            {
                return Ok();
            }

            var response = await _matchingEngineAdapter.CancelLimitOrderAsync(id);
            if (response.Error != null)
            {
                _log.Warning("Cancel limit order", response.Error.Message, context: new { orderId = id.ToString()}.ToJson());

                if (response.Error.Message == "NotFound")
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }
            return Ok();
        }

        /// <summary>
        /// Cancel all open limit orders.
        /// </summary>
        /// <param name="assetPairId">[Optional] Cancel the orders of a specific asset pair</param>
        /// <param name="side">[Optional] Cancel the orders of a specific side (Buy or Sell)</param>
        /// <response code="200">All open limit orders have been canceled</response>
        /// <response code="404">Requested asset pair could not be found or is disabled.</response>
        [HttpDelete]
        [SwaggerOperation(nameof(CancelAll))]
        [ProducesResponseType(typeof(IEnumerable<Guid>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CancelAll(string assetPairId = null, Side? side = null)
        {
            AssetPair assetPair = null;
            if (!string.IsNullOrWhiteSpace(assetPairId))
            {
                assetPair = _assetPairsReadModel.TryGetIfEnabled(assetPairId);
                if (assetPair == null)
                {
                    return NotFound($"Asset-pair '{assetPairId}' could not be found or is disabled.");
                }
            }

            bool? isBuy;
            switch (side)
            {
                case Side.Buy:
                    isBuy = true;
                    break;
                case Side.Sell:
                    isBuy = false;
                    break;
                default:
                    isBuy = null;
                    break;
            }

            var response = await _matchingEngineAdapter.CancelAllAsync(User.GetUserId(), assetPair, isBuy);
            if (response.Error != null)
            {
                return BadRequest(response);
            }

            return Ok();
        }

        private static LimitOrderStateModel ToModel(OrderModel order)
        {
            return new LimitOrderStateModel
            {
                Id = order.Id,
                AssetPairId = order.AssetPairId,
                CreatedAt = order.CreateDt,
                LastMatchTime = order.MatchDt,
                Price = order.Price,
                RemainingVolume = order.RemainingVolume,
                Status = (OrderStatus)order.Status,
                Volume = order.Volume,
                Type = (OrderType)order.Type,
                LowerPrice = order.LowerPrice,
                LowerLimitPrice = order.LowerLimitPrice,
                UpperPrice = order.UpperPrice,
                UpperLimitPrice = order.UpperLimitPrice
            };
        }
    }
}
