using System;
using Lykke.Service.HFT.Core.Domain;
using FeeOrderAction = Lykke.Service.FeeCalculator.AutorestClient.Models.OrderAction;
using MeOrderAction = Lykke.MatchingEngine.Connector.Abstractions.Models.OrderAction;

namespace Lykke.Service.HFT.Services
{
    public static class Mapper
    {
        public static FeeOrderAction ToFeeOrderAction(this OrderAction action)
        {
            FeeOrderAction orderAction;
            switch (action)
            {
                case OrderAction.Buy:
                    orderAction = FeeOrderAction.Buy;
                    break;
                case OrderAction.Sell:
                    orderAction = FeeOrderAction.Sell;
                    break;
                default:
                    throw new Exception("Unknown order action");
            }

            return orderAction;
        }

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
