using System;
using Lykke.Service.HFT.Contracts.Orders;
using MeOrderAction = Lykke.MatchingEngine.Connector.Models.Common.OrderAction;

namespace Lykke.Service.HFT.Services
{
    public static class Mapper
    {
        public static MeOrderAction ToMeOrderAction(this OrderAction action)
        {
            MeOrderAction orderAction;
            switch (action)
            {
                case OrderAction.Buy:
                    orderAction = MeOrderAction.Buy;
                    break;
                case OrderAction.Sell:
                    orderAction = MeOrderAction.Sell;
                    break;
                default:
                    throw new InvalidOperationException("Unknown order action");
            }

            return orderAction;
        }
    }
}
