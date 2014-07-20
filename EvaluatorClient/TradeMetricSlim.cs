using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;
namespace EvaluatorClient
{
    sealed class TradeMetricSlim
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public int ProfitTimes { get; private set; }
        public int TotalTimes { get; private set; }
        public string WinRatio { get; private set; }
        public string Commission { get; private set; }
        public string NetProfit { get; private set; }
        public string ProfitRatio { get; private set; }
        public string AnnualProfitRatio { get; private set; }
        public string MaxDrawDown { get; private set; }
        public string MaxDrawDownRatio { get; private set; }

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
            WinRatio = string.Format("{0:0.00}", metric.ProfitTimesRatio * 100.0);
            Commission = string.Format("{0:0.00}", metric.TotalCommission);
            NetProfit = string.Format("{0:0.00}", metric.NetProfit);
            ProfitRatio = string.Format("{0:0.00}", metric.ProfitRatio * 100.0);
            AnnualProfitRatio = string.Format("{0:0.00}", metric.AnnualProfitRatio * 100.0);
            MaxDrawDown = string.Format("{0:0.00}", metric.MaxDrawDown);
            MaxDrawDownRatio = string.Format("{0:0.00}", metric.MaxDrawDownRatio * 100.0);

            Metric = metric;
        }
    }
}
