using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models;
using Lykke.Service.HFT.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.SwaggerGen;
using OrderStatus = Lykke.Service.HFT.Models.Requests.OrderStatus;

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
        [SwaggerOperation("GetOrders")]
        [ProducesResponseType(typeof(IEnumerable<Models.LimitOrderState>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOrders(
            [FromQuery] OrderStatus? status = null,
            [FromQuery] int? take = 100)
        {
            var toTake = take.ValidateAndGetValue(nameof(take), MaxPageSize, 100);
            if (toTake.Error != null)
            {
                return BadRequest(new ResponseModel { Error = toTake.Error });
            }

            if (!status.HasValue)
            {
                status = OrderStatus.All;
            }

            var clientId = User.GetUserId();

            var states = new List<Core.Domain.OrderStatus>();
            switch (status)
            {
                case OrderStatus.All:
                    break;
                case OrderStatus.Open:
                    states.AddRange(new[] { Core.Domain.OrderStatus.InOrderBook, Core.Domain.OrderStatus.Processing });
                    break;
                case OrderStatus.InOrderBook:
                    states.Add(Core.Domain.OrderStatus.InOrderBook);
                    break;
                case OrderStatus.Processing:
                    states.Add(Core.Domain.OrderStatus.Processing);
                    break;
                case OrderStatus.Matched:
                    states.Add(Core.Domain.OrderStatus.Matched);
                    break;
                case OrderStatus.Cancelled:
                    states.Add(Core.Domain.OrderStatus.Cancelled);
                    break;
                case OrderStatus.Rejected:
                    states = Enum.GetValues(typeof(Core.Domain.OrderStatus))
                        .Cast<Core.Domain.OrderStatus>()
                        .Except(new[]
                        {
                            Core.Domain.OrderStatus.Pending,
                            Core.Domain.OrderStatus.InOrderBook,
                            Core.Domain.OrderStatus.Processing,
                            Core.Domain.OrderStatus.Matched,
                            Core.Domain.OrderStatus.Cancelled
                        })
                        .ToList();
                    break;
            }

            var result = await _orderStateCache.GetOrdersByStatus(clientId, states, toTake.Result);

            return Ok(result.Select(x => x.ConvertToApiModel()));
        }


        /// <summary>
        /// Get the order info.
        /// </summary>
        /// <param name="id">Limit order id</param>
        /// <returns>Order info.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation("GetOrder")]
        [ProducesResponseType(typeof(Models.LimitOrderState), (int)HttpStatusCode.OK)]
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

            return Ok(order.ConvertToApiModel());
        }

        /// <summary>
        /// Place a market order.
        /// </summary>
        /// <returns>Average strike price.</returns>
        [HttpPost("market")]
        [SwaggerOperation("PlaceMarketOrder")]
        [ProducesResponseType(typeof(ResponseModel<double>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel<double>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceMarketOrder([FromBody] MarketOrderRequest order)
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

            return Ok(ResponseModel<double>.CreateOk(response.Result));
        }

        /// <summary>
        /// Place a limit order.
        /// </summary>
        /// <returns>Request id.</returns>
        [HttpPost("limit")]
        [SwaggerOperation("PlaceLimitOrder")]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PlaceLimitOrder([FromBody] LimitOrderRequest order)
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
        /// Cancel the limit order.
        /// </summary>
        /// <param name="id">Limit order id</param>
        [HttpPost("{id}/Cancel")]
        [SwaggerOperation("CancelLimitOrder")]
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

            if (order.Status == Core.Domain.OrderStatus.Cancelled)
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

        private static ResponseModel ToResponseModel(ModelStateDictionary modelState)
        {
            var field = modelState.Keys.First();
            var message = modelState[field].Errors.First().ErrorMessage;
            return ResponseModel.CreateInvalidFieldError(field, message);
        }
    }
}
