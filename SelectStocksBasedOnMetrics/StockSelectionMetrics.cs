using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectStocksBasedOnMetrics
{
    class StockSelectionMetrics
    {
        public enum Movement : int
        {
            NoMove = 0,
            UpCross = 1,
            DownCross = 2,
            PotentialUpCross = 3,
            PotentialDownCross = 4
        }

        public string Code { get; set; }
        public DateTime Date { get; set; }
        public double DayN0CloseMarketPrice { get; set; }
        public double DayN0Ma10 { get; set; }
        public double DayN1Ma10 { get; set; }
        public double DayN2Ma10 { get; set; }
        public double DayN0Ma20 { get; set; }
        public double DayN1Ma20 { get; set; }
        public double DayN2Ma20 { get; set; }
        public double DayN0Atr20 { get; set; }
        public double DayN0StddevAtr20 { get; set; }
        public double DayN0Atr40 { get; set; }
        public double DayN0StddevAtr40 { get; set; }
        public Movement Predication { get; set; }
        public double PredicatedPriceWillCauseMovement { get; set; }
    }
}
