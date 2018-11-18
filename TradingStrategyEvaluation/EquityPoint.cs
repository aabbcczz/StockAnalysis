namespace StockAnalysis.TradingStrategy.Evaluation
{
    using System;

    public class EquityPoint
    {
        // change fields to property to ensure CsvWriter/CsvReader can process it.
        public DateTime Time { get; set; }
        public double Equity { get; set; }
        public double Capital { get; set; }
        public double MaxEquity { get; set; }
        public double DrawDown { get; set; }
        public double DrawDownRatio { get; set; }
    }
}
