using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy
{
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
                case TradingPriceOption.MinOpenPrevClosePrice:
                    price = Math.Min(currentPeriodBar.OpenPrice, previousPeriodBar.ClosePrice);
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
