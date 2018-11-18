using System;
using TradingStrategyEvaluation;

namespace EvaluatorCmdClient
{
    [Serializable]
    public sealed class EvaluationSummary
    {
        public CombinedStrategySettings StrategySettings { get; set; }
        public TradingSettings TradingSettings { get; set; }
        public ChineseStockDataSettings DataSettings { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int YearInterval { get; set; }
        public string[] ObjectNames { get; set; }
    }
}
