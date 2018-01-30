namespace Lykke.Service.HFT.Core.Domain
{
    public enum OrderStatus
    {
        // values 4, 5, 6, 8 are used for DB compatibility reasons only; it should be ME related values

        //Order request is not acknowledged.
        Pending = 0,
        //Init status, limit order in order book
        InOrderBook = 1
        //Partially matched
        , Processing = 2
        //Fully matched
        , Matched = 3
        //Not enough funds on account
        , NotEnoughFunds = 4
        //No liquidity
        , NoLiquidity = 5
        //Unknown asset
        , UnknownAsset = 6
        //Cancelled
        , Cancelled = 7
        //Lead to negative spread
        , LeadToNegativeSpread = 8
        //Reserved volume greater than balance
        , ReservedVolumeGreaterThanBalance = 414
        //Too small volume
        , TooSmallVolume = 418
        //Unexpected status code
        , Runtime = 500
    }
}
