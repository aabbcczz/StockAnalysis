using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using TradingStrategy;
using TradingStrategyEvaluation;
using CsvHelper;
using CsvHelper.Configuration;

namespace EvaluatorCmdClient
{
    public sealed class ResultSummary
    {
        private const int MaxParameterCount = 40;

        public sealed class ResultSummaryMap : CsvClassMap<ResultSummary>
        {
            public ResultSummaryMap()
            {
                AutoMap();

                for (int i = 0; i < MaxParameterCount; ++i)
                {
                    string exp = string.Format("m => m.ParameterValue{0}", i + 1);
                    var e = System.Linq.Dynamic.DynamicExpression.ParseLambda<ResultSummary, object>(exp);

                    if (i >= ParameterNames.Length)
                    {
                        Map(e).Ignore();
                    }
                    else
                    {
                        Map(e).Name(ResultSummary.ParameterNames[i]);
                    }
                }
            }
        }

        public static string[] ParameterNames;
        private static PropertyInfo[] ParameterValueProperties;

        public string ParameterValue1 { get; set; }
        public string ParameterValue2 { get; set; }
        public string ParameterValue3 { get; set; }
        public string ParameterValue4 { get; set; }
        public string ParameterValue5 { get; set; }
        public string ParameterValue6 { get; set; }
        public string ParameterValue7 { get; set; }
        public string ParameterValue8 { get; set; }
        public string ParameterValue9 { get; set; }
        public string ParameterValue10 { get; set; }
        public string ParameterValue11 { get; set; }
        public string ParameterValue12 { get; set; }
        public string ParameterValue13 { get; set; }
        public string ParameterValue14 { get; set; }
        public string ParameterValue15 { get; set; }
        public string ParameterValue16 { get; set; }
        public string ParameterValue17 { get; set; }
        public string ParameterValue18 { get; set; }
        public string ParameterValue19 { get; set; }
        public string ParameterValue20 { get; set; }
        public string ParameterValue21 { get; set; }
        public string ParameterValue22 { get; set; }
        public string ParameterValue23 { get; set; }
        public string ParameterValue24 { get; set; }
        public string ParameterValue25 { get; set; }
        public string ParameterValue26 { get; set; }
        public string ParameterValue27 { get; set; }
        public string ParameterValue28 { get; set; }
        public string ParameterValue29 { get; set; }
        public string ParameterValue30 { get; set; }
        public string ParameterValue31 { get; set; }
        public string ParameterValue32 { get; set; }
        public string ParameterValue33 { get; set; }
        public string ParameterValue34 { get; set; }
        public string ParameterValue35 { get; set; }
        public string ParameterValue36 { get; set; }
        public string ParameterValue37 { get; set; }
        public string ParameterValue38 { get; set; }
        public string ParameterValue39 { get; set; }
        public string ParameterValue40 { get; set; }

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

            ParameterNames = serializableParameterValues.Parameters.Select(p => p.Name).ToArray();

            if (ParameterNames.Length > MaxParameterCount)
            {
                throw new ArgumentException(string.Format("# of parameters exceeds limit {0}", MaxParameterCount));
            }

            ParameterValueProperties = new PropertyInfo[MaxParameterCount];

            for (int i = 0; i < MaxParameterCount; ++i)
            {
                string name = string.Format("ParameterValue{0}", i + 1);

                ParameterValueProperties[i] = typeof(ResultSummary).GetProperty(name);
            }
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

            for (int i = 0; i < serializableParameterValues.Parameters.Length; ++i)
            {
                ParameterValueProperties[i].SetValue(this, serializableParameterValues.Parameters[i].Value);
            }

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
