namespace Lykke.Service.HFT.Services.Events
{
    // Owned by Lykke.Service.ClientAccount service
    public class ClientSettingsCashoutBlockUpdated
    {
        public string ClientId { get; set; }
        public bool CashOutBlocked { get; set; }
        public bool TradesBlocked { get; set; }
    }
}
