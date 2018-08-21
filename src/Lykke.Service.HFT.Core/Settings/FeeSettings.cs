namespace Lykke.Service.HFT.Core.Settings
{
    public class FeeSettings
    {
        public TargetClientIdFeeSettings TargetClientId { get; set; }

        public class TargetClientIdFeeSettings
        {
            public string Hft { get; set; }
        }
    }
}