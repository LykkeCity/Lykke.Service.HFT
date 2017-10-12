// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.Service.HFT.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class MarketOrderRequest
    {
        /// <summary>
        /// Initializes a new instance of the MarketOrderRequest class.
        /// </summary>
        public MarketOrderRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MarketOrderRequest class.
        /// </summary>
        /// <param name="orderAction">Possible values include: 'Buy',
        /// 'Sell'</param>
        public MarketOrderRequest(OrderAction orderAction, double volume, string assetPairId = default(string), string asset = default(string))
        {
            AssetPairId = assetPairId;
            Asset = asset;
            OrderAction = orderAction;
            Volume = volume;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "AssetPairId")]
        public string AssetPairId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Asset")]
        public string Asset { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Buy', 'Sell'
        /// </summary>
        [JsonProperty(PropertyName = "OrderAction")]
        public OrderAction OrderAction { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Volume")]
        public double Volume { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
        }
    }
}
