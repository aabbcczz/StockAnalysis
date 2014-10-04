using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategyEvaluation;

namespace EvaluatorCmdClient
{
    [Serializable]
    public sealed class EvaluationSummary
    {
        public CombinedStrategySettings StrategySettings { get; set; }
        public TradingSettings TradingSettings { get; set; }
        public ChinaStockDataSettings DataSettings { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string[] ObjectNames { get; set; }
    }
}
