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
    }
}
