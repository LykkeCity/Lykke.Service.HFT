using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.Assets;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Service.HFT.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private readonly IMatchingEngineAdapter _matchingEngineAdapter;
        private readonly IAssetPairsManager _assetPairsManager;
        private readonly IRepository<LimitOrderState> _orderStateRepository;
        private readonly IOrderBooksService _orderBooksService;
        private readonly AppSettings _settings;

        public OrdersController(
            IMatchingEngineAdapter frequencyTradingService,
            IAssetPairsManager assetPairsManager,
            IRepository<LimitOrderState> orderStateRepository,
            IOrderBooksService orderBooksService,
            AppSettings settings)
        {
            _matchingEngineAdapter = frequencyTradingService ?? throw new ArgumentNullException(nameof(frequencyTradingService));
            _assetPairsManager = assetPairsManager ?? throw new ArgumentNullException(nameof(assetPairsManager));
            _orderStateRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _orderBooksService = orderBooksService ?? throw new ArgumentNullException(nameof(orderBooksService));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
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

            var assetPair = await _assetPairsManager.TryGetEnabledAssetPairAsync(order.AssetPairId);
            if (assetPair == null)
            {
                var model = ResponseModel<double>.CreateFail(ResponseModel.ErrorCodeType.UnknownAsset);
                return BadRequest(model);
            }

            var asset = await _assetPairsManager.TryGetEnabledAssetAsync(assetPair.BaseAssetId);
            if (asset == null)
            {
                throw new InvalidOperationException($"Base asset '{assetPair.BaseAssetId}' for asset pair '{assetPair.Id}' not found.");
            }

            var clientId = User.GetUserId();
            var response = await _matchingEngineAdapter.HandleMarketOrderAsync(
                clientId: clientId,
                assetPairId: order.AssetPairId,
                orderAction: order.OrderAction,
                volume: order.Volume.TruncateDecimalPlaces(asset.Accuracy),
                straight: order.Asset == assetPair.BaseAssetId,
                reservedLimitVolume: null);

            if (response.Error != null)
            {
                // todo: produce valid http status codes based on ME response 
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

            var assetPair = await _assetPairsManager.TryGetEnabledAssetPairAsync(order.AssetPairId);
            if (assetPair == null)
            {
                var model = ResponseModel.CreateFail(ResponseModel.ErrorCodeType.UnknownAsset);
                return BadRequest(model);
            }

            var currentTopPrice = (decimal)await _orderBooksService.GetBestPrice(order.AssetPairId, order.OrderAction == OrderAction.Buy);
            var deviation = _settings.Exchange.MaxLimitOrderDeviationPercent / 100;
            var price = (decimal)order.Price;
            if (order.OrderAction == OrderAction.Buy && currentTopPrice * (1 - deviation) > price
                || order.OrderAction == OrderAction.Sell && currentTopPrice * (1 + deviation) < price)
            {
                return BadRequest(ResponseModel.CreateFail(ResponseModel.ErrorCodeType.PriceGapTooHigh));
            }

            var asset = await _assetPairsManager.TryGetEnabledAssetAsync(assetPair.BaseAssetId);
            if (asset == null)
            {
                throw new InvalidOperationException($"Base asset '{assetPair.BaseAssetId}' for asset pair '{assetPair.Id}' not found.");
            }

            var clientId = User.GetUserId();
            var response = await _matchingEngineAdapter.PlaceLimitOrderAsync(
                clientId: clientId,
                assetPairId: order.AssetPairId,
                orderAction: order.OrderAction,
                volume: order.Volume.TruncateDecimalPlaces(asset.Accuracy),
                price: order.Price.TruncateDecimalPlaces(assetPair.Accuracy));
            if (response.Error != null)
            {
                // todo: produce valid http status codes based on ME response 
                return BadRequest(response);
            }

            return Ok(response.Result);
        }

        /// <summary>
        /// Cancel the limit order.
        /// </summary>
        /// <param name="id">Limit order id (Guid)</param>
        [HttpPost("{id}/Cancel")]
        [SwaggerOperation("CancelLimitOrder")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
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

            var response = await _matchingEngineAdapter.CancelLimitOrderAsync(id);
            if (response.Error != null)
            {
                // todo: produce valid http status codes based on ME response 
                return NotFound();
            }
            return Ok();
        }

        /// <summary>
        /// Get the order info.
        /// </summary>
        /// <param name="id">Limit order id (Guid)</param>
        /// <returns>Order info.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation("GetOrderInfo")]
        [ProducesResponseType(typeof(LimitOrderState), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetOrderInfo(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }
            var order = await _orderStateRepository.Get(id);
            if (order != null)
            {
                return Ok(order);
            }

            return NotFound();
        }

        private static ResponseModel ToResponseModel(ModelStateDictionary modelState)
        {
            var field = modelState.Keys.First();
            var message = modelState[field].Errors.First().ErrorMessage;
            return ResponseModel.CreateInvalidFieldError(field, message);
        }

    }
}
