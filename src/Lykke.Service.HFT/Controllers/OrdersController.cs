using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models.Requests;
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
        private readonly AppSettings.HighFrequencyTradingSettings _appSettings;
        private readonly IMatchingEngineAdapter _matchingEngineAdapter;
        private readonly IAssetServiceDecorator _assetServiceDecorator;
        private readonly IRepository<LimitOrderState> _orderStateRepository;
        private readonly IOrderBooksService _orderBooksService;
        private readonly double _deviation;

        public OrdersController(
            IMatchingEngineAdapter frequencyTradingService,
            IAssetServiceDecorator assetServiceDecorator,
            IRepository<LimitOrderState> orderStateRepository,
            IOrderBooksService orderBooksService,
            ExchangeSettings exchangeSettings,
            [NotNull] AppSettings.HighFrequencyTradingSettings appSettings)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _matchingEngineAdapter = frequencyTradingService ?? throw new ArgumentNullException(nameof(frequencyTradingService));
            _assetServiceDecorator = assetServiceDecorator ?? throw new ArgumentNullException(nameof(assetServiceDecorator));
            _orderStateRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _orderBooksService = orderBooksService ?? throw new ArgumentNullException(nameof(orderBooksService));

            if (exchangeSettings == null)
                throw new ArgumentNullException(nameof(exchangeSettings));
            _deviation = (double)exchangeSettings.MaxLimitOrderDeviationPercent / 100;
        }

        /// <summary>
        /// Get all client orders.
        /// </summary>
        /// <returns>Client orders.</returns>
        [HttpGet]
        [SwaggerOperation("GetOrders")]
        [ProducesResponseType(typeof(IEnumerable<LimitOrderState>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetOrders([FromQuery] OrderStatus? status = null)
        {
            var clientId = User.GetUserId();
            var orders = status.HasValue
                ? _orderStateRepository.FilterBy(x => x.ClientId == clientId && x.Status == status.Value)
                : _orderStateRepository.FilterBy(x => x.ClientId == clientId);
            return Ok(orders.OrderByDescending(x => x.CreatedAt));
        }

        /// <summary>
        /// Get the order info.
        /// </summary>
        /// <param name="id">Limit order id</param>
        /// <returns>Order info.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation("GetOrder")]
        [ProducesResponseType(typeof(LimitOrderState), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var order = await _orderStateRepository.Get(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
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
            if (assetPair == null)
            {
                var model = ResponseModel<double>.CreateFail(ResponseModel.ErrorCodeType.UnknownAsset);
                return BadRequest(model);
            }
            if (IsAssetPairDisabled(assetPair))
            {
                return BadRequest(ResponseModel.CreateInvalidFieldError("AssetPairId", $"AssetPair {order.AssetPairId} is temporarily disabled"));
            }

            var baseAsset = await _assetServiceDecorator.GetEnabledAssetAsync(assetPair.BaseAssetId);
            var quotingAsset = await _assetServiceDecorator.GetEnabledAssetAsync(assetPair.QuotingAssetId);
            if (order.Asset != baseAsset.Id && order.Asset != baseAsset.Name && order.Asset != quotingAsset.Id && order.Asset != quotingAsset.Name)
            {
                var model = ResponseModel.CreateInvalidFieldError("Asset", $"Asset <{order.Asset}> is not valid for asset pair <{assetPair.Id}>.");
                return BadRequest(model);
            }

            var clientId = User.GetUserId();
            var straight = order.Asset == baseAsset.Id || order.Asset == baseAsset.Name;
            var volume = order.Volume.TruncateDecimalPlaces(straight ? baseAsset.Accuracy : quotingAsset.Accuracy);
            if (Math.Abs(volume) < double.Epsilon)
            {
                var model = ResponseModel<double>.CreateFail(ResponseModel.ErrorCodeType.Dust, "Required volume is less than asset accuracy.");
                return BadRequest(model);
            }
            var response = await _matchingEngineAdapter.HandleMarketOrderAsync(
                clientId: clientId,
                assetPairId: order.AssetPairId,
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
            if (assetPair == null)
            {
                var model = ResponseModel.CreateFail(ResponseModel.ErrorCodeType.UnknownAsset);
                return BadRequest(model);
            }
            if (IsAssetPairDisabled(assetPair))
            {
                return BadRequest(ResponseModel.CreateInvalidFieldError("AssetPairId", $"AssetPair {order.AssetPairId} is temporarily disabled"));
            }

            var bestPrice = await _orderBooksService.GetBestPrice(order.AssetPairId, order.OrderAction == OrderAction.Buy);
            if (bestPrice.HasValue)
            {
                if (order.OrderAction == OrderAction.Buy && bestPrice * (1 - _deviation) > order.Price
                    || order.OrderAction == OrderAction.Sell && bestPrice * (1 + _deviation) < order.Price)
                {
                    return BadRequest(ResponseModel.CreateFail(ResponseModel.ErrorCodeType.PriceGapTooHigh));
                }
            }

            var asset = await _assetServiceDecorator.GetEnabledAssetAsync(assetPair.BaseAssetId);
            if (asset == null)
            {
                throw new InvalidOperationException($"Base asset '{assetPair.BaseAssetId}' for asset pair '{assetPair.Id}' not found.");
            }

            var clientId = User.GetUserId();
            var volume = order.Volume.TruncateDecimalPlaces(asset.Accuracy);
            if (Math.Abs(volume) < double.Epsilon)
            {
                var model = ResponseModel<double>.CreateFail(ResponseModel.ErrorCodeType.Dust, "Required volume is less than asset accuracy.");
                return BadRequest(model);
            }
            var response = await _matchingEngineAdapter.PlaceLimitOrderAsync(
                clientId: clientId,
                assetPairId: order.AssetPairId,
                orderAction: order.OrderAction,
                volume: volume,
                price: order.Price.TruncateDecimalPlaces(assetPair.Accuracy));
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

            var order = await _orderStateRepository.Get(id);
            if (order == null)
            {
                return NotFound();
            }
            if (order.ClientId != User.GetUserId())
            {
                return Forbid();
            }
            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.NoLiquidity || order.Status == OrderStatus.NotEnoughFunds ||
                order.Status == OrderStatus.UnknownAsset || order.Status == OrderStatus.LeadToNegativeSpread || order.Status == OrderStatus.ReservedVolumeGreaterThanBalance)
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

        private bool IsAssetPairDisabled(Assets.Client.Models.AssetPair assetPair)
        {
            return IsAssetDisabled(assetPair.BaseAssetId) || IsAssetDisabled(assetPair.QuotingAssetId);
        }

        private bool IsAssetDisabled(string asset)
        {
            return _appSettings.DisabledAssets.Contains(asset);
        }
    }
}
