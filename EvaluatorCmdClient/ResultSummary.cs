using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

using TradingStrategy;
using TradingStrategyEvaluation;
using CsvHelper.Configuration;

namespace EvaluatorCmdClient
{
    public sealed class ResultSummary
    {
        private const int MaxParameterCount = 80;
        private const int MaxERatioCount = 40;

        public sealed class ResultSummaryMap : CsvClassMap<ResultSummary>
        {
            public ResultSummaryMap()
            {
                AutoMap();

                for (var i = 0; i < MaxParameterCount; ++i)
                {
                    var exp = string.Format("m => m.ParameterValue{0}", i + 1);
                    var e = DynamicExpression.ParseLambda<ResultSummary, object>(exp);

                    if (i >= ParameterNames.Length)
                    {
                        Map(e).Ignore();
                    }
                    else
                    {
                        Map(e).Name(ParameterNames[i]);
                    }
                }

                for (var i = 0; i < MaxERatioCount; ++i)
                {
                    var exp = string.Format("m => m.ERatio{0}", i + 1);
                    var e = DynamicExpression.ParseLambda<ResultSummary, object>(exp);

                    if (i >= TradeMetricsCalculator.ERatioWindowSizes.Length || !ResultSummary.ShouldOutputERatio)
                    {
                        Map(e).Ignore();
                    }
                    else
                    {
                        Map(e).Name(string.Format("ERatio{0}", TradeMetricsCalculator.ERatioWindowSizes[i]));
                    }
                }
            }
        }

        public static string[] ParameterNames;
        private static PropertyInfo[] _parameterValueProperties;
        private static PropertyInfo[] _eRatioProperties;

        public static bool ShouldOutputERatio;

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
        public string ParameterValue41 { get; set; }
        public string ParameterValue42 { get; set; }
        public string ParameterValue43 { get; set; }
        public string ParameterValue44 { get; set; }
        public string ParameterValue45 { get; set; }
        public string ParameterValue46 { get; set; }
        public string ParameterValue47 { get; set; }
        public string ParameterValue48 { get; set; }
        public string ParameterValue49 { get; set; }
        public string ParameterValue50 { get; set; }
        public string ParameterValue51 { get; set; }
        public string ParameterValue52 { get; set; }
        public string ParameterValue53 { get; set; }
        public string ParameterValue54 { get; set; }
        public string ParameterValue55 { get; set; }
        public string ParameterValue56 { get; set; }
        public string ParameterValue57 { get; set; }
        public string ParameterValue58 { get; set; }
        public string ParameterValue59 { get; set; }
        public string ParameterValue60 { get; set; }
        public string ParameterValue61 { get; set; }
        public string ParameterValue62 { get; set; }
        public string ParameterValue63 { get; set; }
        public string ParameterValue64 { get; set; }
        public string ParameterValue65 { get; set; }
        public string ParameterValue66 { get; set; }
        public string ParameterValue67 { get; set; }
        public string ParameterValue68 { get; set; }
        public string ParameterValue69 { get; set; }
        public string ParameterValue70 { get; set; }
        public string ParameterValue71 { get; set; }
        public string ParameterValue72 { get; set; }
        public string ParameterValue73 { get; set; }
        public string ParameterValue74 { get; set; }
        public string ParameterValue75 { get; set; }
        public string ParameterValue76 { get; set; }
        public string ParameterValue77 { get; set; }
        public string ParameterValue78 { get; set; }
        public string ParameterValue79 { get; set; }
        public string ParameterValue80 { get; set; }

        public double ERatio1 { get; set; }
        public double ERatio2 { get; set; }
        public double ERatio3 { get; set; }
        public double ERatio4 { get; set; }
        public double ERatio5 { get; set; }
        public double ERatio6 { get; set; }
        public double ERatio7 { get; set; }
        public double ERatio8 { get; set; }
        public double ERatio9 { get; set; }
        public double ERatio10 { get; set; }
        public double ERatio11 { get; set; }
        public double ERatio12 { get; set; }
        public double ERatio13 { get; set; }
        public double ERatio14 { get; set; }
        public double ERatio15 { get; set; }
        public double ERatio16 { get; set; }
        public double ERatio17 { get; set; }
        public double ERatio18 { get; set; }
        public double ERatio19 { get; set; }
        public double ERatio20 { get; set; }
        public double ERatio21 { get; set; }
        public double ERatio22 { get; set; }
        public double ERatio23 { get; set; }
        public double ERatio24 { get; set; }
        public double ERatio25 { get; set; }
        public double ERatio26 { get; set; }
        public double ERatio27 { get; set; }
        public double ERatio28 { get; set; }
        public double ERatio29 { get; set; }
        public double ERatio30 { get; set; }
        public double ERatio31 { get; set; }
        public double ERatio32 { get; set; }
        public double ERatio33 { get; set; }
        public double ERatio34 { get; set; }
        public double ERatio35 { get; set; }
        public double ERatio36 { get; set; }
        public double ERatio37 { get; set; }
        public double ERatio38 { get; set; }
        public double ERatio39 { get; set; }
        public double ERatio40 { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
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
        public DateTime MaxDrawDownStartTime { get; set; } // 最大回撤发生的起始时间
        public DateTime MaxDrawDownEndTime { get; set; } // 最大回撤发生的结束时间
        public double MaxDrawDownRatio { get; set; } // 最大回撤比率 =（前期高点-低点）/前期高点
        public DateTime MaxDrawDownRatioStartTime { get; set; } // 最大回撤比率发生的起始时间
        public DateTime MaxDrawDownRatioEndTime { get; set; } // 最大回撤比率发生的结束时间
        public double Mar { get; set; } // = AnnualProfitRatio / MaxDrawDownRatio
        public double Expectation { get; set; } // = AverageProfitPerTrading * ProfitTimesRatio - (1 - ProfitTimesRatio) * AverageLossPerTrading;
        public double BestFactor { get; set; } // best factor according to Kelly formular: f = (bp-q)/b.
        public int ContextId { get; set; }
        public string ContextDirectory { get; set; }

        public static void Initialize(IDictionary<Tuple<int, ParameterAttribute>, object> parameterValues, bool shouldOutputERatio)
        {
            if (parameterValues == null)
            {
                throw new ArgumentNullException();
            }

            var serializableParameterValues = new SerializableParameterValues();
            serializableParameterValues.Initialize(parameterValues);

            ParameterNames = serializableParameterValues.Parameters.Select(p => p.Name).ToArray();

            if (ParameterNames.Length > MaxParameterCount)
            {
                throw new ArgumentException(string.Format("# of parameters exceeds limit {0}", MaxParameterCount));
            }

            _parameterValueProperties = new PropertyInfo[MaxParameterCount];

            for (var i = 0; i < MaxParameterCount; ++i)
            {
                var name = string.Format("ParameterValue{0}", i + 1);

                _parameterValueProperties[i] = typeof(ResultSummary).GetProperty(name);
            }

            ResultSummary.ShouldOutputERatio = shouldOutputERatio;

            _eRatioProperties = new PropertyInfo[MaxERatioCount];

            for (var i = 0; i < MaxERatioCount; ++i)
            {
                var name = string.Format("ERatio{0}", i + 1);

                _eRatioProperties[i] = typeof(ResultSummary).GetProperty(name);
            }
        }

        public void Initialize(
            EvaluationResultContext context,
            IDictionary<Tuple<int, ParameterAttribute>, object> parameterValues, 
            DateTime startDate,
            DateTime endDate,
            TradeMetric metric)
        {
            if (context == null || parameterValues == null || metric == null)
            {
                throw new ArgumentNullException();
            }

            var serializableParameterValues = new SerializableParameterValues();
            serializableParameterValues.Initialize(parameterValues);

            for (var i = 0; i < serializableParameterValues.Parameters.Length; ++i)
            {
                _parameterValueProperties[i].SetValue(this, serializableParameterValues.Parameters[i].Value);
            }

            for (var i = 0; i < metric.ERatios.Length; ++i)
            {
                _eRatioProperties[i].SetValue(this, metric.ERatios[i]);
            }

            StartDate = startDate;
            EndDate = endDate;
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
            MaxDrawDownStartTime = metric.MaxDrawDownStartTime;
            MaxDrawDownEndTime = metric.MaxDrawDownEndTime;
            MaxDrawDownRatio = metric.MaxDrawDownRatio;
            MaxDrawDownRatioStartTime = metric.MaxDrawDownRatioStartTime;
            MaxDrawDownRatioEndTime = metric.MaxDrawDownRatioEndTime;
            Mar = metric.Mar;
            Expectation = ProfitTimesRatio * AverageProfitPerTrading - (1.0 - ProfitTimesRatio) * AverageLossPerTrading;

            var b = AverageProfitPerTrading / AverageLossPerTrading;
            BestFactor = (b * ProfitTimesRatio - (1.0 - ProfitTimesRatio)) / b;

            ContextId = context.ContextId;
            ContextDirectory = context.RootDirectory;
        }
    }
}
