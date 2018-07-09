using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Contracts.Orders;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private const int MaxPageSize = 500;
        private readonly RequestValidator _requestValidator;
        private readonly IMatchingEngineAdapter _matchingEngineAdapter;
        private readonly IAssetServiceDecorator _assetServiceDecorator;
        private readonly ILimitOrderStateRepository _orderStateCache;
        private readonly ILimitOrderStateArchive _orderStateArchive;

        public OrdersController(
            IMatchingEngineAdapter frequencyTradingService,
            IAssetServiceDecorator assetServiceDecorator,
            ILimitOrderStateRepository orderStateCache,
            ILimitOrderStateArchive orderStateArchive,
            RequestValidator requestValidator)
        {
            _matchingEngineAdapter = frequencyTradingService ?? throw new ArgumentNullException(nameof(frequencyTradingService));
            _assetServiceDecorator = assetServiceDecorator ?? throw new ArgumentNullException(nameof(assetServiceDecorator));
            _orderStateCache = orderStateCache ?? throw new ArgumentNullException(nameof(orderStateCache));
            _orderStateArchive = orderStateArchive ?? throw new ArgumentNullException(nameof(orderStateArchive));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
        }

        /// <summary>
        /// Get the last client orders.
        /// </summary>
        /// <param name="status">Order status</param>
        /// <param name="take">The amount of orders to take, default 100; max 500.</param>
        /// <returns>Client orders.</returns>
        [HttpGet]
        [SwaggerOperation(nameof(GetOrders))]
        [ProducesResponseType(typeof(IEnumerable<LimitOrderStateModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOrders(
            [FromQuery] OrderStatusQuery? status = null,
            [FromQuery] int? take = 100)
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

            var clientId = User.GetUserId();

            var states = new List<OrderStatus>();
            switch (status)
            {
                case OrderStatusQuery.All:
                    break;
                case OrderStatusQuery.Open:
                    states.AddRange(new[]
                    {
                        OrderStatus.Pending,
                        OrderStatus.InOrderBook,
                        OrderStatus.Processing
                    });
                    break;
                case OrderStatusQuery.InOrderBook:
                    states.Add(OrderStatus.InOrderBook);
                    break;
                case OrderStatusQuery.Processing:
                    states.Add(OrderStatus.Processing);
                    break;
                case OrderStatusQuery.Matched:
                    states.Add(OrderStatus.Matched);
                    break;
                case OrderStatusQuery.Cancelled:
                    states.Add(OrderStatus.Cancelled);
                    break;
                case OrderStatusQuery.Rejected:
                    states = Enum.GetValues(typeof(OrderStatus))
                        .Cast<OrderStatus>()
                        .Except(new[]
                        {
                            OrderStatus.Pending,
                            OrderStatus.InOrderBook,
                            OrderStatus.Processing,
                            OrderStatus.Matched,
                            OrderStatus.Cancelled
                        })
                        .ToList();
                    break;
            }

            var result = await _orderStateCache.GetOrdersByStatus(clientId, states, toTake.Result);

            return Ok(result.Select(ToModel));
        }


        /// <summary>
        /// Get the order info.
        /// </summary>
        /// <param name="id">Limit order id</param>
        /// <returns>Order info.</returns>
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

            var order = await _orderStateCache.Get(id) as ILimitOrderState;

            if (order == null)
            {
                var clientId = User.GetUserId();
                order = await _orderStateArchive.GetAsync(clientId, id);
                if (order == null)
                {
                    return NotFound();
                }
            }

            return Ok(ToModel(order));
        }

        /// <summary>
        /// Place a market order.
        /// </summary>
        /// <returns>Average strike price.</returns>
        // TODO make this method obsolete and introduce a v2 that returns a DTO instead of ResponseModel{double}
        [HttpPost("market")]
        [SwaggerOperation(nameof(PlaceMarketOrder))]
        [ProducesResponseType(typeof(ResponseModel<double>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceMarketOrder([FromBody] PlaceMarketOrderModel order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ToResponseModel(ModelState));
            }

            var assetPair = await _assetServiceDecorator.GetEnabledAssetPairAsync(order.AssetPairId);
            if (!_requestValidator.ValidateAssetPair(order.AssetPairId, assetPair, out var badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var baseAsset = await _assetServiceDecorator.GetEnabledAssetAsync(assetPair.BaseAssetId);
            var quotingAsset = await _assetServiceDecorator.GetEnabledAssetAsync(assetPair.QuotingAssetId);
            if (!_requestValidator.ValidateAsset(assetPair, order.Asset, baseAsset, quotingAsset, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var straight = order.Asset == baseAsset.Id || order.Asset == baseAsset.DisplayId;
            var asset = straight ? baseAsset : quotingAsset;
            var volume = order.Volume.TruncateDecimalPlaces(asset.Accuracy);
            var minVolume = straight ? assetPair.MinVolume : assetPair.MinInvertedVolume;
            if (!_requestValidator.ValidateVolume(volume, minVolume, asset.DisplayId, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var clientId = User.GetUserId();
            var response = await _matchingEngineAdapter.HandleMarketOrderAsync(
                clientId: clientId,
                assetPair: assetPair,
                orderAction: order.OrderAction,
                volume: volume,
                straight: straight,
                reservedLimitVolume: null);

            if (response.Error != null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Place a limit order.
        /// </summary>
        /// <returns>Request id.</returns>
        [HttpPost("limit")]
        [SwaggerOperation(nameof(PlaceLimitOrder))]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceLimitOrder([FromBody] PlaceLimitOrderModel order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ToResponseModel(ModelState));
            }

            var assetPair = await _assetServiceDecorator.GetEnabledAssetPairAsync(order.AssetPairId);

            if (!_requestValidator.ValidateAssetPair(order.AssetPairId, assetPair, out var badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var asset = await _assetServiceDecorator.GetEnabledAssetAsync(assetPair.BaseAssetId);
            if (asset == null)
                throw new InvalidOperationException($"Base asset '{assetPair.BaseAssetId}' for asset pair '{assetPair.Id}' not found.");

            var price = order.Price.TruncateDecimalPlaces(assetPair.Accuracy);
            if (!_requestValidator.ValidatePrice(price, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var volume = order.Volume.TruncateDecimalPlaces(asset.Accuracy);
            var minVolume = assetPair.MinVolume;
            if (!_requestValidator.ValidateVolume(volume, minVolume, asset.DisplayId, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var clientId = User.GetUserId();
            var response = await _matchingEngineAdapter.PlaceLimitOrderAsync(
                clientId: clientId,
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
        /// Place a bulk limit order.
        /// </summary>
        /// <returns>Request id.</returns>
        [HttpPost("bulk")]
        [SwaggerOperation(nameof(PlaceBulkOrder))]
        [ProducesResponseType(typeof(BulkOrderResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceBulkOrder([FromBody] PlaceBulkOrderModel order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ToResponseModel(ModelState));
            }

            var assetPair = await _assetServiceDecorator.GetEnabledAssetPairAsync(order.AssetPairId);

            if (!_requestValidator.ValidateAssetPair(order.AssetPairId, assetPair, out var badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var asset = await _assetServiceDecorator.GetEnabledAssetAsync(assetPair.BaseAssetId);
            if (asset == null)
                throw new InvalidOperationException($"Base asset '{assetPair.BaseAssetId}' for asset pair '{assetPair.Id}' not found.");

            var items = order.Orders?.ToArray() ?? new BulkOrderItemModel[0];
            foreach (var item in items)
            {
                var price = item.Price.TruncateDecimalPlaces(assetPair.Accuracy);
                if (!_requestValidator.ValidatePrice(price, out badRequestModel))
                {
                    return BadRequest(badRequestModel);
                }

                var volume = item.Volume.TruncateDecimalPlaces(asset.Accuracy);
                var minVolume = assetPair.MinVolume;
                if (!_requestValidator.ValidateVolume(volume, minVolume, asset.DisplayId, out badRequestModel))
                {
                    return BadRequest(badRequestModel);
                }

                item.Price = price;
                item.Volume = volume;
            }

            var clientId = User.GetUserId();
            var response = await _matchingEngineAdapter.PlaceBulkLimitOrderAsync(
                clientId: clientId,
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
        /// Cancel the limit order.
        /// </summary>
        /// <param name="id">Limit order id</param>
        [HttpPost("{id}/Cancel")]
        [SwaggerOperation(nameof(CancelLimitOrderOld))]
        [Obsolete("Use http delete {id} instead.")]
        public Task<IActionResult> CancelLimitOrderOld(Guid id)
            => CancelLimitOrder(id);

        /// <summary>
        /// Cancel the limit order.
        /// </summary>
        /// <param name="id">Limit order id</param>
        [HttpDelete("{id}")]
        [SwaggerOperation(nameof(CancelLimitOrder))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CancelLimitOrder(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var clientId = User.GetUserId();

            var order = await _orderStateCache.Get(id) as ILimitOrderState;
            if (order == null)
            {
                order = await _orderStateArchive.GetAsync(clientId, id);
                if (order == null)
                {
                    return NotFound();
                }
            }
            if (order.ClientId != clientId)
            {
                return Forbid();
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                return Ok();
            }
            // if rejected, do nothing
            if (order.Status.IsRejected())
            {
                return Ok();
            }

            var response = await _matchingEngineAdapter.CancelLimitOrderAsync(id);
            if (response.Error != null)
            {
                return BadRequest(response);
            }
            return Ok();
        }

        /// <summary>
        /// Cancels all open limit orders.
        /// </summary>
        [HttpDelete]
        [SwaggerOperation(nameof(CancelAll))]
        [ProducesResponseType(typeof(IEnumerable<Guid>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CancelAll(
            [FromQuery] string assetPairId = null,
            [FromQuery] Side? side = null)
        {
            AssetPair assetPair = null;
            if (!string.IsNullOrWhiteSpace(assetPairId))
            {
                assetPair = await _assetServiceDecorator.GetEnabledAssetPairAsync(assetPairId);
                if (assetPair == null)
                {
                    return NotFound($"Assetpair '{assetPairId}' could not be found or is disabled.");
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

        private static LimitOrderStateModel ToModel(ILimitOrderState order)
        {
            return new LimitOrderStateModel
            {
                Id = order.Id,
                AssetPairId = order.AssetPairId,
                CreatedAt = order.CreatedAt,
                LastMatchTime = order.LastMatchTime,
                Price = order.Price,
                RemainingVolume = order.RemainingVolume,
                Status = order.Status,
                Volume = order.Volume
            };
        }

        private static ResponseModel ToResponseModel(ModelStateDictionary modelState)
        {
            var field = modelState.Keys.First();
            var message = modelState[field].Errors.First().ErrorMessage;
            return ResponseModel.CreateInvalidFieldError(field, message);
        }
    }
}
