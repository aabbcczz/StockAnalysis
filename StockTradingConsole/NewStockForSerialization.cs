﻿namespace StockTradingConsole
{
    using System;

    public sealed class NewStockForSerialization
    {
        public DateTime DateToBuy { get; set; }
        public string SecuritySymbol { get; set; }
        public string SecurityName { get; set; }
        public float BuyPriceUpLimitInclusive { get; set; }
        public float BuyPriceDownLimitInclusive { get; set; }
        public float TotalCapitalUsedToBuy { get; set; }

        public NewStockForSerialization()
        {
        }

        public NewStockForSerialization(NewStock ns)
        {
            DateToBuy = ns.DateToBuy;
            SecuritySymbol = ns.Name.Symbol.NormalizedSymbol;
            SecurityName = ns.Name.Names[0];
            BuyPriceUpLimitInclusive = ns.BuyPriceUpLimitInclusive;
            BuyPriceDownLimitInclusive = ns.BuyPriceDownLimitInclusive;
            TotalCapitalUsedToBuy = ns.TotalCapitalUsedToBuy;
        }
    }
}
