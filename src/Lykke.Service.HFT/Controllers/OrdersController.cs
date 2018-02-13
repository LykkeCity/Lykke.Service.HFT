using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Helpers;
using Lykke.Service.HFT.Models;
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
        private const int MaxPageSize = 1000;
        private readonly RequestValidator _requestValidator;
        private readonly IMatchingEngineAdapter _matchingEngineAdapter;
        private readonly IAssetServiceDecorator _assetServiceDecorator;
        private readonly IRepository<Core.Domain.LimitOrderState> _orderStateRepository;

        public OrdersController(
            IMatchingEngineAdapter frequencyTradingService,
            IAssetServiceDecorator assetServiceDecorator,
            IRepository<Core.Domain.LimitOrderState> orderStateRepository,
            [NotNull] RequestValidator requestValidator)
        {
            _matchingEngineAdapter = frequencyTradingService ?? throw new ArgumentNullException(nameof(frequencyTradingService));
            _assetServiceDecorator = assetServiceDecorator ?? throw new ArgumentNullException(nameof(assetServiceDecorator));
            _orderStateRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
        }

        /// <summary>
        /// Get all client orders.
        /// </summary>
        /// <returns>Client orders.</returns>
        [HttpGet]
        [SwaggerOperation("GetOrders")]
        [ProducesResponseType(typeof(IEnumerable<Models.LimitOrderState>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOrders([FromQuery] OrderStatus? status = null, [FromQuery] uint? skip = 0, [FromQuery] uint? take = 100)
        {
            if (take > MaxPageSize)
            {
                return BadRequest(ResponseModel.CreateInvalidFieldError("take", $"Page size {take} is to big"));
            }

            var clientId = User.GetUserId();
            var orders = status.HasValue
                ? _orderStateRepository.All().Where(x => x.ClientId == clientId && x.Status == status.Value)
                : _orderStateRepository.All().Where(x => x.ClientId == clientId);
            return Ok(orders.OrderByDescending(x => x.CreatedAt).Skip((int)skip.Value).Take((int)take.Value).ToList().Select(x => x.ConvertToApiModel()));
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

            var order = await _orderStateRepository.Get(id);
            if (order == null)
            {
                return NotFound();
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

            var straight = order.Asset == baseAsset.Id || order.Asset == baseAsset.Name;
            var volume = order.Volume.TruncateDecimalPlaces(straight ? baseAsset.Accuracy : quotingAsset.Accuracy);
            var minVolume = straight ? assetPair.MinVolume : assetPair.MinInvertedVolume;
            if (!_requestValidator.ValidateVolume(volume, minVolume, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var clientId = User.GetUserId();
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
            if (!_requestValidator.ValidateAssetPair(order.AssetPairId, assetPair, out var badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var asset = await _assetServiceDecorator.GetEnabledAssetAsync(assetPair.BaseAssetId);
            if (asset == null)
                throw new InvalidOperationException($"Base asset '{assetPair.BaseAssetId}' for asset pair '{assetPair.Id}' not found.");

            var straight = order.OrderAction == OrderAction.Buy;
            var volume = order.Volume.TruncateDecimalPlaces(asset.Accuracy);
            var minVolume = straight ? assetPair.MinVolume : assetPair.MinInvertedVolume;
            if (!_requestValidator.ValidateVolume(volume, minVolume, out badRequestModel))
            {
                return BadRequest(badRequestModel);
            }

            var clientId = User.GetUserId();
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
                order.Status == OrderStatus.UnknownAsset || order.Status == OrderStatus.LeadToNegativeSpread)
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
