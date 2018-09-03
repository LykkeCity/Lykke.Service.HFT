namespace Lykke.Service.HFT.Core.Domain
{
    public class AssetPair
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Accuracy { get; set; }
        public int InvertedAccuracy { get; set; }
        public string BaseAssetId { get; set; }
        public string QuotingAssetId { get; set; }
        public double MinVolume { get; set; }
        public double MinInvertedVolume { get; set; }
        public bool IsDisabled { get; set; }
    }
}
