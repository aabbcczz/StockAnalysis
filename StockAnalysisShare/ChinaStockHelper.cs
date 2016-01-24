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
    }
}
