using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectStocksBasedOnMetrics
{
    class StockDailyMetrics
    {
        //"code,date,close,atr.20,stddev.atr.20,atr.40,stddev.atr.40,ma.5,ma.10,ma.20,ma.30,ma.60"
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public double CloseMarketPrice { get; set; }
        public double Atr20 { get; set; }
        public double StddevAtr20 { get; set; }
        public double Atr40 { get; set; }
        public double StddevAtr40 { get; set; }
        public double Ma5 { get; set; }
        public double Ma10 { get; set; }
        public double Ma20 { get; set; }
        public double Ma30 { get; set; }
        public double Ma60 { get; set; }
    }
}
