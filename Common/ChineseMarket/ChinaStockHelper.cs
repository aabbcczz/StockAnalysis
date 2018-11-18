namespace StockAnalysis.Common.ChineseMarket
{
    public static class ChinaStockHelper
    {
        public const int VolumePerHand = 100;
        public const double DefaultUpLimitPercentage = 10.0F;
        public const double DefaultDownLimitPercentage = -10.0F;
        public const double SpecialTreatmentUpLimitPercentage = 5.0F;
        public const double SpecialTreatmentDownLimitPercentage = -5.0F;

        public static bool IsSpecialTreatmentStock(string symbol, string name)
        {
            if (name.StartsWith("*ST") || name.StartsWith("ST"))
            {
                return true;
            }

            return false;
        }

        public static int ConvertVolumeToHand(int volume)
        {
            return (volume + VolumePerHand / 2) / VolumePerHand;
        }

        public static long ConvertVolumeToHand(long volume)
        {
            return (volume + VolumePerHand / 2) / VolumePerHand;
        }

        public static int ConvertHandToVolume(int volumeInHand)
        {
            return volumeInHand * VolumePerHand;
        }

        public static long ConvertHandToVolume(long volumeInHand)
        {
            return volumeInHand * VolumePerHand;
        }

        public static double GetUpLimitPercentage(string symbol, string name)
        {
            if (IsSpecialTreatmentStock(symbol, name))
            {
                return SpecialTreatmentUpLimitPercentage;
            }
            else
            {
                return DefaultUpLimitPercentage;
            }
        }
        public static double GetDownLimitPercentage(string symbol, string name)
        {
            if (IsSpecialTreatmentStock(symbol, name))
            {
                return SpecialTreatmentDownLimitPercentage;
            }
            else
            {
                return DefaultDownLimitPercentage;
            }
        }

        public static double CalculatePrice(double price, double changePercentage, int roundPosition)
        {
            if (double.IsNaN(price))
            {
                return double.NaN;
            }

            decimal changedPrice = (decimal)price * (100.0m + (decimal)changePercentage) / 100.0m;

            decimal roundedPrice = decimal.Round(changedPrice, roundPosition);

            return (double)roundedPrice;
        }

        public static double CalculateUpLimit(double price, double upLimitPercentage, int roundPosition)
        {
            return CalculatePrice(price, upLimitPercentage, roundPosition);
        }

        public static double CalculateUpLimit(string symbol, string name, double price, int roundPosition)
        {
            return CalculatePrice(price, ChinaStockHelper.GetUpLimitPercentage(symbol, name), roundPosition);
        }

        public static double CalculateDownLimit(double price, double downLimitPercentage, int roundPoisition)
        {
            return CalculatePrice(price, downLimitPercentage, roundPoisition);
        }

        public static double CalculateDownLimit(string symbol, string name, double price, int roundPosition)
        {
            return CalculatePrice(price, ChinaStockHelper.GetDownLimitPercentage(symbol, name), roundPosition);
        }
    }
}
