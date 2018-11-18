namespace StockAnalysis.TradingStrategy.Evaluation
{
    using System;
    using System.Linq;
    using TradingStrategy;

    using MathNet.Numerics;
    using MathNet.Numerics.Statistics;

    public sealed class TradeMetric
    {
        public const string SymbolForAll = "-----";
        public const string NameForAll = "全部对象";

        public string Symbol { get; set; } // if Symbol is SymbolForAll, it represents all trading objects.
        public string Name { get; set; }
        public DateTime StartDate { get; set; } // 统计起始日期
        public DateTime EndDate { get; set; }  // 统计结束日期
        public int TotalTradingDays { get; set; } // 总交易天数 = EndDate - StartDate + 1 { get; set; }

        public double InitialEquity { get; set; } // 期初权益
        public double FinalEquity { get; set; } // 期末权益

        public int TotalPeriods { get; set; } // 总周期数
        public int ProfitPeriods { get; set; } // 盈利周期数, 即本周期权益比上一周期上升
        public int LossPeriods { get; set; } // 亏损周期数，即本周期权益比上一周期下降
        public double ProfitLossPeriodRatio { get; set; } // 盈亏周期比，=ProfitPeriods/LossPeriods { get; set; }

        public double TotalProfit { get; set; } // 总盈利
        public double TotalLoss { get; set; } // 总亏损
        public double TotalCommission { get; set; } // 总手续费
        public double NetProfit { get; set; } // 净利润 = TotalProfit - TotalLoss - TotalCommission
        public double ProfitRatio { get; set; } // 收益率 = NetProfit / InitialEquity
        public double AnnualProfitRatio { get; set; } // 年化收益率 = ProfitRatio / TotalTradingDays * 365

        public int TotalTradingTimes { get; set; } // 总交易次数，每次卖出算做一次交易 = ProfitTradingTimes + LossTradingTimes
        public int ProfitTradingTimes { get; set; } // 盈利交易次数
        public int LossTradingTimes { get; set; } // 亏损交易次数
        public double ProfitCoefficient { get; set; } // 盈利系数 = (ProfitTradingTimes - LossTradingTimes) / TotalTradingTimes { get; set; }
        public double ProfitTimesRatio { get; set; } // 胜率 = ProfitTradingTimes / TradingTimes { get; set; }
        public double LossTimesRatio { get; set; } // 亏损比率 = LossTradingTimes / TotalTradingTimes { get; set; }
        public double ProfitLossTimesRatio { get; set; } // 盈亏次数比 = ProfitTradingTimes / LossTradingTimes { get; set; }
        public double AverageProfitPerTrading { get; set; } // 每交易平均盈利 = TotalProfit / ProfitTradingTimes;
        public double AverageLossPerTrading { get; set; } // 每交易平均亏损 = TotalLoss / LossTradingTimes;

        public long TotalVolume { get; set; } // 总交易量 = ProfitVolume + LossVolume
        public long ProfitVolume { get; set; } // 盈利量, 所有盈利交易中交易量的总和
        public long LossVolume { get; set; } // 亏损量，所以亏损交易中交易量的总和
        public double AverageProfitPerVolume { get; set; } // 每量平均盈利 = TotalProfit / ProfitVolume
        public double AverageLossPerVolume { get; set; } // 每量平均亏损 = TotalLoss / LossVolume

        public double MaxDrawDown { get; set; } // 最大回撤 =（前期高点-低点）
        public DateTime MaxDrawDownStartTime { get; set; } // 最大回撤发生的起始时间
        public DateTime MaxDrawDownEndTime { get; set;  } // 最大回撤发生的结束时间
        public double MaxDrawDownStartEquity { get; set; } // 最大回撤发生的期初权益
        public double MaxDrawDownEndEquity { get; set; } // 最大回撤发生的期末权益
        public double MaxDrawDownRatio { get; set; } // 最大回测比率 =（前期高点-低点）/前期高点
        public DateTime MaxDrawDownRatioStartTime { get; set; } // 最大回撤比率发生的起始时间
        public DateTime MaxDrawDownRatioEndTime { get; set; } // 最大回撤比率发生的结束时间
        public double MaxDrawDownRatioStartEquity { get; set; } // 最大回撤比率发生的期初权益
        public double MaxDrawDownRatioEndEquity { get; set; } // 最大回撤比率发生的期末权益

        public double Mar { get; set; } // = AnnualProfitRatio / MaxDrawDownRatio

        public double AnnualSharpeRatio { get; set; } // Sharpe ratio

        public double KRatio { get; set; } // K-Ratio

        public double MaxProfitInOneTransaction { get; set; } // 单次最大盈利
        public double MaxLossInOneTransaction { get; set; } // 单次最大亏损
        public double ProfitRatioWithoutMaxProfit { get; set; } // 扣除单次最大盈利后的收益率 = (NetProfit - MaxProfitInOneTransaction) / InitialEquity
        public double ProfitRatioWithoutMaxLoss { get; set; }  // 扣除单次最大亏损后的收益率 = (NetProfit + MaxLossInOneTransaction) / InitialEquity

        public double StartPrice { get; set; } // 品种起始价格 
        public double EndPrice { get; set; } // 品种结束价格
        public double Rise { get; set; } // 区间涨幅 = (EndPrice - StartPrice) / StartPrice 

        public EquityPoint[] OrderedEquitySequence { get; private set; } // 所有权益按周期排序
        public CompletedTransaction[] OrderedCompletedTransactionSequence { get; private set; } // all completed transactions, ordered by execution time and symbol
        public Transaction[] OrderedTransactionSequence { get; private set; }
        public double[] ERatios { get; private set; }

        public void Initialize(
            string symbol,
            string name,
            DateTime startDate,
            DateTime endDate,
            double startPrice,
            double endPrice,
            EquityPoint[] orderedEquitySequence, 
            CompletedTransaction[] orderedCompletedTransactionSequence,
            Transaction[] orderedTransactionSequence,
            double[] eRatios,
            bool exponentialEquity = true)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (startDate.Date != startDate)
            {
                throw new ArgumentException("startDate is not a day");
            }

            if (endDate.Date != endDate)
            {
                throw new ArgumentException("endDate is not a day");
            }

            if (startDate >= endDate)
            {
                throw new ArgumentException("startDate must be earlier than endDate");
            }

            if (orderedEquitySequence == null)
            {
                throw new ArgumentNullException("orderedEquitySequence");
            }

            if (orderedCompletedTransactionSequence == null)
            {
                throw new ArgumentNullException("orderedCompletedTransactionSequence");
            }

            if (orderedTransactionSequence == null)
            {
                throw new ArgumentNullException("orderedTransactionSequence");
            }

            if (eRatios == null)
            {
                throw new ArgumentNullException("eRatios");
            }

            if (orderedEquitySequence.Length == 0)
            {
                throw new ArgumentException("equitySequence does not contain data");
            }

            if (orderedEquitySequence[0].Time < startDate)
            {
                throw new ArgumentOutOfRangeException("the smallest time in equitySequence is eariler than startDate");
            }

            if (orderedEquitySequence[orderedEquitySequence.Length - 1].Time > endDate)
            {
                throw new ArgumentOutOfRangeException("the largest time in equitySequence is later than endDate");
            }

            Symbol = symbol;
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
            
            OrderedEquitySequence = orderedEquitySequence;

            OrderedCompletedTransactionSequence = orderedCompletedTransactionSequence;
            OrderedTransactionSequence = orderedTransactionSequence;
            ERatios = eRatios;

            StartPrice = startPrice;
            EndPrice = endPrice;
            Rise = Math.Abs(startPrice) < 1e-6 ? 0.0 : (endPrice - startPrice) / startPrice;

            Initialize(exponentialEquity);
        }

        private void Initialize(bool exponentialEquity)
        {
            // calculate max drawdown for each equity point
            double maxEquity = OrderedEquitySequence[0].Equity;
            for (int i = 0; i < OrderedEquitySequence.Length; ++i)
            {
                EquityPoint ep = OrderedEquitySequence[i];
                maxEquity = Math.Max(ep.Equity, maxEquity);

                ep.MaxEquity = maxEquity;
                ep.DrawDown = maxEquity - ep.Equity;
                ep.DrawDownRatio = ep.DrawDown / maxEquity;
            }

            // update trading days
            TotalTradingDays = (int)(EndDate.Subtract(StartDate).TotalDays + 1.0);

            // update initial/final equity
            InitialEquity = OrderedEquitySequence[0].Equity;
            FinalEquity = OrderedEquitySequence[OrderedEquitySequence.Length - 1].Equity;

            // update total/profit/loss periods and profit/loss period ratio
            TotalPeriods = OrderedEquitySequence.Length;
            ProfitPeriods = 0;
            LossPeriods = 0;
            var previousEquity = double.NaN;
            foreach (var e in OrderedEquitySequence)
            {
                if (!double.IsNaN(previousEquity))
                {
                    if (e.Equity > previousEquity)
                    {
                        ++ProfitPeriods;
                    }
                    else if (e.Equity < previousEquity)
                    {
                        ++LossPeriods;
                    }
                }

                previousEquity = e.Equity;
            }

            ProfitLossPeriodRatio = LossPeriods == 0 ? 0.0 : (double)ProfitPeriods / LossPeriods;


            // update profit related metrics
            TotalProfit = OrderedCompletedTransactionSequence.Sum(ct => ct.SoldGain > ct.BuyCost ? ct.SoldGain - ct.BuyCost : 0.0);
            TotalLoss = OrderedCompletedTransactionSequence.Sum(ct => ct.SoldGain < ct.BuyCost ? ct.BuyCost - ct.SoldGain : 0.0);
            TotalCommission = OrderedCompletedTransactionSequence.Sum(ct => ct.Commission);
            NetProfit = TotalProfit - TotalLoss - TotalCommission;
            ProfitRatio = NetProfit / InitialEquity;

            var reciprocalYears = 365.0 / TotalTradingDays;
            AnnualProfitRatio = Math.Pow(1.0 + ProfitRatio, reciprocalYears) - 1.0;

            // update trading times related metrics
            TotalTradingTimes = OrderedCompletedTransactionSequence.Count();
            ProfitTradingTimes = OrderedCompletedTransactionSequence.Count(ct => ct.SoldGain > ct.BuyCost);
            LossTradingTimes = OrderedCompletedTransactionSequence.Count(ct => ct.SoldGain < ct.BuyCost);
            ProfitCoefficient = TotalTradingTimes == 0 ? 0.0 : (double)(ProfitTradingTimes - LossTradingTimes) / TotalTradingTimes;
            ProfitTimesRatio = TotalTradingTimes == 0 ? 0.0 : (double)ProfitTradingTimes / TotalTradingTimes;
            LossTimesRatio = TotalTradingTimes == 0 ? 0.0 : (double)LossTradingTimes / TotalTradingTimes;
            ProfitLossTimesRatio = LossTradingTimes == 0 ? 0.0 : (double)ProfitTradingTimes / LossTradingTimes;
            AverageProfitPerTrading = ProfitTradingTimes == 0 ? 0.0 : TotalProfit / ProfitTradingTimes;
            AverageLossPerTrading = LossTradingTimes == 0 ? 0.0 : TotalLoss / LossTradingTimes;

            // update trading volume related metrics
            TotalVolume = OrderedCompletedTransactionSequence.Sum(ct => (long)ct.Volume);
            ProfitVolume = OrderedCompletedTransactionSequence.Sum(ct => ct.SoldGain > ct.BuyCost ? (long)ct.Volume : 0);
            LossVolume = OrderedCompletedTransactionSequence.Sum(ct => ct.SoldGain < ct.BuyCost ? (long)ct.Volume : 0);
            AverageProfitPerVolume = ProfitVolume == 0 ? 0.0 : TotalProfit / ProfitVolume;
            AverageLossPerVolume = LossVolume == 0 ? 0.0 : TotalLoss / LossVolume;

            // update max drawdown related metrics
            CalcuateMaxDrawDownMetrics();

            // update max profit/loss related metrics
            if (OrderedCompletedTransactionSequence.Any())
            {
                MaxProfitInOneTransaction = OrderedCompletedTransactionSequence.Max(ct => ct.SoldGain > ct.BuyCost ? ct.SoldGain - ct.BuyCost : 0.0);
                MaxLossInOneTransaction = OrderedCompletedTransactionSequence.Max(ct => ct.SoldGain < ct.BuyCost ? ct.BuyCost - ct.SoldGain : 0.0);
            }
            else
            {
                MaxProfitInOneTransaction = 0.0;
                MaxLossInOneTransaction = 0.0;
            }

            ProfitRatioWithoutMaxProfit = (NetProfit - MaxProfitInOneTransaction) / InitialEquity;
            ProfitRatioWithoutMaxLoss = (NetProfit + MaxProfitInOneTransaction) / InitialEquity;

            CalculateSharpeRatio();

            CalculateKRatio(exponentialEquity);
        }

        private void CalcuateMaxDrawDownMetrics()
        {
            MaxDrawDown = OrderedEquitySequence.Max(ep => ep.DrawDown);
            int indexOfMaxDrawDown = Enumerable
                .Range(0, OrderedEquitySequence.Length)
                .First(i => OrderedEquitySequence[i].DrawDown == MaxDrawDown);

            int indexOfMaxEquityOfMaxDrawDown = indexOfMaxDrawDown == 0
                ? 0
                : Enumerable
                    .Range(0, indexOfMaxDrawDown)
                    .First(i => OrderedEquitySequence[i].MaxEquity == OrderedEquitySequence[indexOfMaxDrawDown].MaxEquity);

            MaxDrawDownStartEquity = OrderedEquitySequence[indexOfMaxEquityOfMaxDrawDown].Equity;
            MaxDrawDownStartTime = OrderedEquitySequence[indexOfMaxEquityOfMaxDrawDown].Time;
            MaxDrawDownEndEquity = OrderedEquitySequence[indexOfMaxDrawDown].Equity;
            MaxDrawDownEndTime = OrderedEquitySequence[indexOfMaxDrawDown].Time;

            MaxDrawDownRatio = OrderedEquitySequence.Max(ep => ep.DrawDownRatio);
            int indexOfMaxDrawDownRatio = Enumerable
                .Range(0, OrderedEquitySequence.Length)
                .First(i => OrderedEquitySequence[i].DrawDownRatio == MaxDrawDownRatio);

            int indexOfMaxEquityOfMaxDrawDownRatio = indexOfMaxDrawDownRatio == 0
                ? 0
                : Enumerable
                    .Range(0, indexOfMaxDrawDownRatio)
                    .First(i => OrderedEquitySequence[i].MaxEquity == OrderedEquitySequence[indexOfMaxDrawDownRatio].MaxEquity);

            MaxDrawDownRatioStartEquity = OrderedEquitySequence[indexOfMaxEquityOfMaxDrawDownRatio].Equity;
            MaxDrawDownRatioStartTime = OrderedEquitySequence[indexOfMaxEquityOfMaxDrawDownRatio].Time;
            MaxDrawDownRatioEndEquity = OrderedEquitySequence[indexOfMaxDrawDownRatio].Equity;
            MaxDrawDownRatioEndTime = OrderedEquitySequence[indexOfMaxDrawDownRatio].Time;
                
            // update MAR
            Mar = Math.Abs(MaxDrawDownRatio) < 1e-6 ? 0.0 : AnnualProfitRatio / MaxDrawDownRatio;
        }

        private void CalculateSharpeRatio()
        {
            double[] periodReturns = new double[OrderedEquitySequence.Length];
            periodReturns[0] = 0;

            for (int i = 1; i < OrderedEquitySequence.Length; ++i)
            {
                periodReturns[i] = (OrderedEquitySequence[i].Equity - OrderedEquitySequence[i - 1].Equity) / OrderedEquitySequence[i - 1].Equity;
            }

            var averageReturn = periodReturns.Average();
            var varianceReturn = periodReturns.Average(r => r * r) - averageReturn * averageReturn;
            var stddevReturn = Math.Sqrt(varianceReturn);

            AnnualSharpeRatio = averageReturn / stddevReturn * Math.Sqrt(252);
        }

        private void CalculateKRatio(bool exponentialEquity)
        {
            double[] equities = exponentialEquity
                ? OrderedEquitySequence.Select(e => Math.Log(e.Equity)).ToArray()
                : OrderedEquitySequence.Select(e => e.Equity).ToArray();
            double[] indices = Enumerable.Range(0, equities.Length).Select(n => (double)n).ToArray();

            if (equities.Count() <= 2)
            {
                KRatio = 1;
            }
            else
            {
                // we are using the KRatio 2013 formula. 
                // see https://thesystematictrader.com/2013/04/22/coding-lars-kestners-k-ratio-in-excel/

                var fitResult = Fit.Line(indices, equities);
                var intercept = fitResult.Item1;
                var slope = fitResult.Item2;

                var standardErrorOfFit = GoodnessOfFit.StandardError(indices.Select(i => intercept + i * slope), equities, 2);
                var sumOfSquareOfX = indices.Variance() * equities.Count();
                var stdErrorOfSlope = standardErrorOfFit / (indices.StandardDeviation() * Math.Sqrt((double)equities.Count()));

                KRatio = slope / stdErrorOfSlope / equities.Count();
            }
        }
    }
}
