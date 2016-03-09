using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public static class ChinaStockHelper
    {
        public const int VolumePerHand = 100;
        public const float DefaultUpLimitPercentage = 10.0F;
        public const float DefaultDownLimitPercentage = -10.0F;
        public const float SpecialTreatmentUpLimitPercentage = 5.0F;
        public const float SpecialTreatmentDownLimitPercentage = -5.0F;

        public static bool IsSpecialTreatmentStock(string code, string name)
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

        public static float GetUpLimitPercentage(string code, string name)
        {
            if (IsSpecialTreatmentStock(code, name))
            {
                return SpecialTreatmentUpLimitPercentage;
            }
            else
            {
                return DefaultUpLimitPercentage;
            }
        }
        public static float GetDownLimitPercentage(string code, string name)
        {
            if (IsSpecialTreatmentStock(code, name))
            {
                return SpecialTreatmentDownLimitPercentage;
            }
            else
            {
                return DefaultDownLimitPercentage;
            }
        }

        public static float CalculatePrice(float price, float changePercentage, int roundPosition)
        {
            if (float.IsNaN(price))
            {
                return float.NaN;
            }

            decimal changedPrice = (decimal)price * (100.0m + (decimal)changePercentage) / 100.0m;

            decimal roundedPrice = decimal.Round(changedPrice, roundPosition);

            return (float)roundedPrice;
        }

        public static float CalculateUpLimit(float price, float upLimitPercentage, int roundPosition)
        {
            return CalculatePrice(price, upLimitPercentage, roundPosition);
        }

        public static float CalculateUpLimit(string code, string name, float price, int roundPosition)
        {
            return CalculatePrice(price, ChinaStockHelper.GetUpLimitPercentage(code, name), roundPosition);
        }

        public static float CalculateDownLimit(float price, float downLimitPercentage, int roundPoisition)
        {
            return CalculatePrice(price, downLimitPercentage, roundPoisition);
        }

        public static float CalculateDownLimit(string code, string name, float price, int roundPosition)
        {
            return CalculatePrice(price, ChinaStockHelper.GetDownLimitPercentage(code, name), roundPosition);
        }
    }
}
