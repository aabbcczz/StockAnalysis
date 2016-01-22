using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class FiveLevelQuote
    {
        private static string[] columns = new string[]
        {
            "证券代码",
            "证券名称",
            "昨收价",
            "今开价",
            "当前价",
            "买一价",
            "买二价",
            "买三价",
            "买四价",
            "买五价",
            "买一量",
            "买二量",
            "买三量",
            "买四量",
            "买五量",
            "卖一价",
            "卖二价",
            "卖三价",
            "卖四价",
            "卖五价",
            "卖一量",
            "卖二量",
            "卖三量",
            "卖四量",
            "卖五量",
        };

        private static int[] columnIndices = null;

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecurityCode { get; private set; }

        /// <summary>
        /// 证券名称
        /// </summary>
        public string SecurityName { get; private set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public float YesterdayClosePrice { get; private set; }

        /// <summary>
        /// 今日开盘价
        /// </summary>
        public float TodayOpenPrice { get; private set; }

        /// <summary>
        /// 现价
        /// </summary>
        public float CurrentPrice { get; private set; }

        /// <summary>
        /// 买入价
        /// </summary>
        public float[] BuyPrices { get; private set; } 

        /// <summary>
        /// 买入量
        /// </summary>
        public int[] BuyVolumesInHand { get; private set; }

        /// <summary>
        /// 卖出价
        /// </summary>
        public float[] SellPrices { get; private set; }

        /// <summary>
        /// 卖出量
        /// </summary>
        public int[] SellVolumesInHand { get; private set; }

        /// <summary>
        /// 至今成交量
        /// </summary>
        public long DealVolumeInHand { get; set; }

        /// <summary>
        /// 至今成交额
        /// </summary>
        public float DealAmount { get; set; }

        public FiveLevelQuote()
        {
            Timestamp = DateTime.Now;
        }

        public FiveLevelQuote(DateTime timestamp)
        {
            Timestamp = timestamp;
        }

        public static IEnumerable<FiveLevelQuote> ExtractFrom(TabulateData data)
        {
            return ExtractFrom(data, DateTime.Now);
        }

        public static IEnumerable<FiveLevelQuote> ExtractFrom(TabulateData data, DateTime timestamp)
        {
            if (columnIndices == null)
            {
                columnIndices = columns.Select(c => data.GetColumnIndex(c)).ToArray();
            }

            var subData = data.GetSubColumns(columnIndices);

            foreach (var row in subData.Rows)
            {
                FiveLevelQuote quote = new FiveLevelQuote(timestamp);

                int index = 0;
                quote.SecurityCode = row[index++];
                quote.SecurityName = row[index++];
                quote.YesterdayClosePrice = TradingHelper.SafeParseFloat(row[index++]);
                quote.TodayOpenPrice = TradingHelper.SafeParseFloat(row[index++]);
                quote.CurrentPrice = TradingHelper.SafeParseFloat(row[index++]);
                quote.BuyPrices = new float[5];
                quote.BuyVolumesInHand = new int[5];
                quote.SellPrices = new float[5];
                quote.SellVolumesInHand = new int[5];

                quote.BuyPrices[0] = TradingHelper.SafeParseFloat(row[index++]);
                quote.BuyPrices[1] = TradingHelper.SafeParseFloat(row[index++]);
                quote.BuyPrices[2] = TradingHelper.SafeParseFloat(row[index++]);
                quote.BuyPrices[3] = TradingHelper.SafeParseFloat(row[index++]);
                quote.BuyPrices[4] = TradingHelper.SafeParseFloat(row[index++]);

                quote.BuyVolumesInHand[0] = TradingHelper.SafeParseInt(row[index++]);
                quote.BuyVolumesInHand[1] = TradingHelper.SafeParseInt(row[index++]);
                quote.BuyVolumesInHand[2] = TradingHelper.SafeParseInt(row[index++]);
                quote.BuyVolumesInHand[3] = TradingHelper.SafeParseInt(row[index++]);
                quote.BuyVolumesInHand[4] = TradingHelper.SafeParseInt(row[index++]);

                quote.SellPrices[0] = TradingHelper.SafeParseFloat(row[index++]);
                quote.SellPrices[1] = TradingHelper.SafeParseFloat(row[index++]);
                quote.SellPrices[2] = TradingHelper.SafeParseFloat(row[index++]);
                quote.SellPrices[3] = TradingHelper.SafeParseFloat(row[index++]);
                quote.SellPrices[4] = TradingHelper.SafeParseFloat(row[index++]);

                quote.SellVolumesInHand[0] = TradingHelper.SafeParseInt(row[index++]);
                quote.SellVolumesInHand[1] = TradingHelper.SafeParseInt(row[index++]);
                quote.SellVolumesInHand[2] = TradingHelper.SafeParseInt(row[index++]);
                quote.SellVolumesInHand[3] = TradingHelper.SafeParseInt(row[index++]);
                quote.SellVolumesInHand[4] = TradingHelper.SafeParseInt(row[index++]);

                yield return quote;
            }
        }
    }
}
