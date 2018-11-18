namespace StockTradingConsole
{
    using System;
    using StockAnalysis.Common.SymbolName;
    using StockAnalysis.Common.ChineseMarket;

    public sealed class NewStock
    {
        public DateTime DateToBuy { get; set; }
        public StockName Name { get; set; }
        public float BuyPriceUpLimitInclusive { get; set; }
        public float BuyPriceDownLimitInclusive { get; set; }
        public float TotalCapitalUsedToBuy { get; set; }
        public float ActualOpenPrice { get; set; }

        public NewStock()
        {
        }

        public NewStock(NewStockForSerialization nss)
        {
            DateToBuy = nss.DateToBuy;
            Name = new StockName(nss.SecuritySymbol, nss.SecurityName);
            BuyPriceUpLimitInclusive = nss.BuyPriceUpLimitInclusive;
            BuyPriceDownLimitInclusive = nss.BuyPriceDownLimitInclusive;
            TotalCapitalUsedToBuy = nss.TotalCapitalUsedToBuy;
            ActualOpenPrice = 0.0f;
        }

        public int BuyableVolumeInHand(float price)
        {
            int volume = (int)(TotalCapitalUsedToBuy / price);

            int hand = ChinaStockHelper.ConvertVolumeToHand(volume);

            return hand;
        }

        public bool IsPriceAcceptable(float price)
        {
            return BuyPriceDownLimitInclusive <= price && price <= BuyPriceUpLimitInclusive;
        }
    }
}
