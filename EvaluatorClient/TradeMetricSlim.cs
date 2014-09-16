using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategyEvaluation;

namespace EvaluatorClient
{
    sealed class TradeMetricSlim
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public int ProfitTimes { get; private set; }
        public int TotalTimes { get; private set; }
        public double WinRatio { get; private set; }
        public double Commission { get; private set; }
        public double NetProfit { get; private set; }
        public double ProfitRatio { get; private set; }
        public double AnnualProfitRatio { get; private set; }
        public double MaxDrawDown { get; private set; }
        public double MaxDrawDownRatio { get; private set; }

        public TradeMetric Metric { get; private set; }

        public TradeMetricSlim(TradeMetric metric)
        {
            if (metric == null)
            {
                throw new ArgumentNullException("metric");
            }

            Code = metric.Code;
            Name = metric.Name;
            ProfitTimes = metric.ProfitTradingTimes;
            TotalTimes = metric.TotalTradingTimes;
            WinRatio = metric.ProfitTimesRatio * 100.0;
            Commission = metric.TotalCommission;
            NetProfit = metric.NetProfit;
            ProfitRatio = metric.ProfitRatio * 100.0;
            AnnualProfitRatio = metric.AnnualProfitRatio * 100.0;
            MaxDrawDown = metric.MaxDrawDown;
            MaxDrawDownRatio = metric.MaxDrawDownRatio * 100.0;

            Metric = metric;
        }
    }
}
