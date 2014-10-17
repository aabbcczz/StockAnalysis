using System;

namespace TradingStrategyEvaluation
{
    public struct EquityPoint
    {
        // change fields to property to ensure CsvWriter/CsvReader can process it.
        public DateTime Time { get; set; }
        public double Equity { get; set; }
        public double Capital { get; set; }
    }
}
