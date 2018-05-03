namespace Lykke.Service.HFT.Models
{
    public class ClientBalanceResponseModel
    {
        public string AssetId { get; set; }
        public decimal Balance { get; set; }
        public decimal Reserved { get; set; }

        public static ClientBalanceResponseModel Create(Balances.AutorestClient.Models.ClientBalanceResponseModel src)
        {
            return new ClientBalanceResponseModel
            {
                AssetId = src.AssetId,
                Balance = src.Balance,
                Reserved = src.Reserved
            };
        }
    }
}
