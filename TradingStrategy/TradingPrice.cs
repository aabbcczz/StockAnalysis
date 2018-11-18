namespace StockAnalysis.TradingStrategy
{
    using System;
    using Common.Data;

    public sealed class TradingPrice
    {
        public TradingPricePeriod Period { get; private set; }
        public TradingPriceOption Option { get; private set; }
        public double CustomPrice { get; private set; }

        public TradingPrice(TradingPricePeriod period, TradingPriceOption option, double customPrice)
        {
            Period = period;
            Option = option;
            CustomPrice = customPrice;
        }

        public double GetRealPrice(
            Bar currentPeriodBar,
            Bar previousPeriodBar)
        {
            double price;

            switch (Option)
            {
                case TradingPriceOption.OpenPrice:
                    price = currentPeriodBar.OpenPrice;
                    break;
                case TradingPriceOption.ClosePrice:
                    price = currentPeriodBar.ClosePrice;
                    break;
                case TradingPriceOption.CustomPrice:
                    price = CustomPrice;
                    break;
                default:
                    throw new InvalidProgramException("Logic error");
            }

            return price;
        }        
    }
}
