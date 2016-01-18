using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    static class TradingHelper
    {
        public const float DefaultUpLimitPercentage = 10.0F;
        public const float DefaultDownLimitPercentage = -10.0F;

        public static bool IsValidPrice(float price)
        {
            return !float.IsNaN(price);
        }

        public static float SafeParseFloat(string s)
        {
            float price;

            if (float.TryParse(s, out price))
            {
                return price;
            }
            else
            {
                return float.NaN;
            }
        }

        public static int SafeParseInt(string s)
        {
            int volume;

            if (int.TryParse(s, out volume))
            {
                return volume;
            }
            else
            {
                return 0;
            }
        }


        public static float CalcLimit(float price, float limitPercentage)
        {
            if (float.IsNaN(price))
            {
                return float.NaN;
            }

            decimal limitF = (decimal)price * 10.0m * (100.0m + (decimal)limitPercentage);

            Int64 limit = (Int64)limitF;
            
            int residential = (int)(limit % 10);

            if (residential < 5)
            {
                limit -= residential;
            }
            else
            {
                limit += 10 - residential;
            }

            return (float)((decimal)limit / 1000.0m);
        }

        public static float CalcUpLimit(float price)
        {
            return CalcLimit(price, DefaultUpLimitPercentage);
        }

        public static float CalcDownLimit(float price)
        {
            return CalcLimit(price, DefaultDownLimitPercentage);
        }
    }
}
