using Lykke.Service.HFT.Core.Domain;
using System;
using MeOrderAction = Lykke.MatchingEngine.Connector.Abstractions.Models.OrderAction;

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
                    throw new Exception("Unknown order action");
            }

            return orderAction;
        }
    }
}
