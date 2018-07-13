using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Assets
{
    /// <summary>
    /// Asset pair response model for asset pairs requests.
    /// </summary>
    /// <remarks>Asset pair can be reverted when placing market orders eg from BTC/USD to USD/BTC in that case inverted accuracy and volumens should be used.</remarks>
    [PublicAPI]
    public class AssetPairModel
    {
        /// <summary>
        /// The asset-pair identifier, eg BTCUSD.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The base asset id, eg BTC in case of BTC/USD.
        /// </summary>
        public string BaseAssetId { get; set; }

        /// <summary>
        /// The quoting asset id, eg USD in case of BTC/USD.
        /// </summary>
        public string QuotingAssetId { get; set; }

        /// <summary>
        /// The name of the asset pair, eg. BTC/USD.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The decimal accuracy of the asset pair.
        /// </summary>
        public int Accuracy { get; set; }

        /// <summary>
        /// The inverted decimal accuracy of the asset pair.
        /// </summary>
        public int InvertedAccuracy { get; set; }

        /// <summary>
        /// The minimum required volume when placing an order.
        /// </summary>
        public double MinVolume { get; set; }

        /// <summary>
        /// The inverted minimum required volume when placing an order.
        /// </summary>
        public double MinInvertedVolume { get; set; }
    }
}
