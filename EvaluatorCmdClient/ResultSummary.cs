using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;
using TradingStrategyEvaluation;
using CsvHelper;
using CsvHelper.Configuration;

namespace EvaluatorCmdClient
{
    public sealed class ResultSummary
    {
        public sealed class ResultSummaryMap : CsvClassMap<ResultSummary>
        {
            public ResultSummaryMap()
            {
                AutoMap();

                Map(m => m.ParameterValuesString).Name(ResultSummary.ParameterHeader);
            }
        }

        private const string ParameterSeparator = "|";

        public static string ParameterHeader;

        public string ParameterValuesString { get; set; }
        public double InitialEquity { get; set; } // 期初权益
        public double FinalEquity { get; set; } // 期末权益
        public double NetProfit { get; set; } // 净利润 = TotalProfit - TotalLoss - TotalCommission
        public double ProfitRatio { get; set; } // 收益率 = NetProfit / InitialEquity
        public double AnnualProfitRatio { get; set; } // 年化收益率 = ProfitRatio / TotalTradingDays * 365
        public int TotalTradingTimes { get; set; } // 总交易次数，每次卖出算做一次交易 = ProfitTradingTimes + LossTradingTimes
        public double ProfitTimesRatio { get; set; } // 胜率 = ProfitTradingTimes / TradingTimes { get; set; }
        public double AverageProfitPerTrading { get; set; } // 每交易平均盈利 = TotalProfit / ProfitTradingTimes;
        public double AverageLossPerTrading { get; set; } // 每交易平均亏损 = TotalLoss / LossTradingTimes;
        public double MaxDrawDown { get; set; } // 最大回撤 =（前期高点-低点）
        public double MaxDrawDownRatio { get; set; } // 最大回测比率 =（前期高点-低点）/前期高点
        public double MAR { get; set; } // = AnnualProfitRatio / MaxDrawDownRatio
        public int ContextId { get; set; }
        public string ContextDirectory { get; set; }

        public static void Initialize(IDictionary<ParameterAttribute, object> parameterValues)
        {
            if (parameterValues == null)
            {
                throw new ArgumentNullException();
            }

            SerializableParameterValues serializableParameterValues = new SerializableParameterValues();
            serializableParameterValues.Initialize(parameterValues);

            ParameterHeader = string.Join(
                ParameterSeparator,
                serializableParameterValues.Parameters.Select(p => p.Name));
        }

        public void Initialize(
            EvaluationResultContext context,
            IDictionary<ParameterAttribute, object> parameterValues, 
            TradeMetric metric)
        {
            if (context == null || parameterValues == null || metric == null)
            {
                throw new ArgumentNullException();
            }

            SerializableParameterValues serializableParameterValues = new SerializableParameterValues();
            serializableParameterValues.Initialize(parameterValues);

            ParameterValuesString = string.Join(
                ParameterSeparator,
                serializableParameterValues.Parameters.Select(p => p.Value));

            InitialEquity = metric.InitialEquity;
            FinalEquity = metric.FinalEquity;
            NetProfit = metric.NetProfit;
            ProfitRatio = metric.ProfitRatio;
            AnnualProfitRatio = metric.AnnualProfitRatio;
            TotalTradingTimes = metric.TotalTradingTimes;
            ProfitTimesRatio = metric.ProfitTimesRatio;
            AverageProfitPerTrading = metric.AverageProfitPerTrading;
            AverageLossPerTrading = metric.AverageLossPerTrading;
            MaxDrawDown = metric.MaxDrawDown;
            MaxDrawDownRatio = metric.MaxDrawDownRatio;
            MAR = metric.MAR;

            ContextId = context.ContextId;
            ContextDirectory = context.RootDirectory;
        }
    }
}
