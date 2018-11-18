namespace StockAnalysis.Common.Utility
{
    using System;
    using ChineseMarket;

    //0：”大秦铁路”，股票名字；
    // 1：”27.55″，今日开盘价；
    // 2：”27.25″，昨日收盘价；
    // 3：”26.91″，当前价格；
    // 4：”27.55″，今日最高价；
    // 5：”26.20″，今日最低价；
    // 6：”26.91″，竞买价，即“买一”报价；
    // 7：”26.92″，竞卖价，即“卖一”报价；
    // 8：”22114263″，成交的股票数，由于股票交易以一百股为基本单位，所以在使用时，通常把该值除以一百；
    // 9：”589824680″，成交金额，单位为“元”，为了一目了然，通常以“万元”为成交金额的单位，所以通常把该值除以一万；
    // 10：”4695″，“买一”申请4695股，即47手；
    // 11：”26.91″，“买一”报价；
    // 12：”57590″，“买二”
    // 13：”26.90″，“买二”
    // 14：”14700″，“买三”
    // 15：”26.89″，“买三”
    // 16：”14300″，“买四”
    // 17：”26.88″，“买四”
    // 18：”15100″，“买五”
    // 19：”26.87″，“买五”
    // 20：”3100″，“卖一”申报3100股，即31手；
    // 21：”26.92″，“卖一”报价
    // (22, 23), (24, 25), (26,27), (28, 29)分别为“卖二”至“卖四的情况”
    // 30：”2008-01-11″，日期；
    // 31：”15:05:32″，时间；
    // 32 "00"

    public sealed class SinaStockQuote
    {
        public string SecuritySymbol { get; set; }
        public string SecurityName { get; set; }
        public float TodayOpenPrice { get; set; }
        public float YesterdayClosePrice { get; set; }
        public float CurrentPrice { get; set; }
        public float TodayHighestPrice { get; set; }
        public float TodayLowestPrice { get; set; }
        public long DealVolumeInHand { get; set; }
        public float DealAmount { get; set; }
        public float[] BuyPrices { get; set; }
        public int[] BuyVolumesInHand { get; set; }
        public float[] SellPrices { get; set; }
        public int[] SellVolumesInHand { get; set; }
        public DateTime QuoteTime { get; set; }

        public SinaStockQuote(string symbol, string rawInput)
        {
            const int fieldCount = 33;

            BuyPrices = new float[5];
            BuyVolumesInHand = new int[5];
            SellPrices = new float[5];
            SellVolumesInHand = new int[5];

            SecuritySymbol = symbol;

            string[] fields = rawInput.Split(',');
            if (fields.Length != fieldCount)
            {
                throw new ArgumentException(
                    string.Format("There is no exact {0} fields in input string", fieldCount));
            }

            int index = 0;
            SecurityName = fields[index++];
            TodayOpenPrice = float.Parse(fields[index++]);
            YesterdayClosePrice = float.Parse(fields[index++]);
            CurrentPrice = float.Parse(fields[index++]);
            TodayHighestPrice = float.Parse(fields[index++]);
            TodayLowestPrice = float.Parse(fields[index++]);
            
            // skip bid 1 and ask 1 because it will repeat in following fields
            index++;
            index++;

            DealVolumeInHand = ChinaStockHelper.ConvertVolumeToHand(long.Parse(fields[index++]));
            DealAmount = float.Parse(fields[index++]);

            for (int i = 0; i < 5; ++i)
            {
                BuyVolumesInHand[i] = ChinaStockHelper.ConvertVolumeToHand(int.Parse(fields[index++]));
                BuyPrices[i] = float.Parse(fields[index++]);
            }

            for (int i = 0; i < 5; ++i)
            {
                SellVolumesInHand[i] = ChinaStockHelper.ConvertVolumeToHand(int.Parse(fields[index++]));
                SellPrices[i] = float.Parse(fields[index++]);
            }

            DateTime date = DateTime.Parse(fields[index++]);
            DateTime time = DateTime.Parse(fields[index++]);

            QuoteTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);

            // skip last field
            index++;
            System.Diagnostics.Debug.Assert(index == fieldCount);
        }
    }
}
