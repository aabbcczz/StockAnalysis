using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class TradeMetric
    {
        public const string CodeForAll = "-----";

        public string Code { get; private set; } // if Code is CodeForAll, it represents all trading objects.
        public DateTime StartDate { get; private set; } // 统计起始日期
        public DateTime EndDate { get; private set; }  // 统计结束日期
        public int TotalTradingDays { get; private set; } // 总交易天数 = EndDate - StartDate + 1 { get; private set; }

        public double InitialEquity { get; private set; } // 初始权益
        public double FinalEquity { get; private set; } // 期末权益

        public int TotalPeriods { get; private set; } // 总周期数
        public int ProfitPeriods { get; private set; } // 盈利周期数, 即本周期权益比上一周期上升
        public int LossPeriods { get; private set; } // 亏损周期数，即本周期权益比上一周期下降
        public double ProfitLossPeriodRatio { get; private set; } // 盈亏周期比，=ProfitPeriods/LossPeriods { get; private set; }

        public double TotalProfit { get; private set; } // 总盈利
        public double TotalLoss { get; private set; } // 总亏损
        public double TotalCommission { get; private set; } // 总手续费
        public double NetProfit { get; private set; } // 净利润 = TotalProfit - TotalLoss - TotalCommission
        public double ProfitRatio { get; private set; } // 收益率 = (NetProfit - InitialEquity) / InitialEquity { get; private set; }
        public double AnnualProfitRatio { get; private set; } // 年化收益率 = ProfitRatio / TotalTradingDays * 365

        public int TotalTradingTimes { get; private set; } // 总交易次数，每次卖出算做一次交易 = ProfitTradingTimes + LossTradingTimes
        public int ProfitTradingTimes { get; private set; } // 盈利交易次数
        public int LossTradingTimes { get; private set; } // 亏损交易次数
        public double ProfitCoefficient { get; private set; } // 盈利系数 = (ProfitTradingTimes - LossTradingTimes) / TotalTradingTimes { get; private set; }
        public double ProfitTimesRatio { get; private set; } // 胜率 = ProfitTradingTimes / TradingTimes { get; private set; }
        public double LossTimesRatio { get; private set; } // 亏损比率 = LossTradingTimes / TotalTradingTimes { get; private set; }
        public double ProfitLossTimesRatio { get; private set; } // 盈亏次数比 = ProfitTradingTimes / LossTradingTimes { get; private set; }

        public int TotalVolume { get; private set; } // 总交易量 = ProfitVolume + LossVolume
        public int ProfitVolume { get; private set; } // 盈利量, 所有盈利交易中交易量的总和
        public int LossVolume { get; private set; } // 亏损量，所以亏损交易中交易量的总和
        public double AverageProfit { get; private set; } // 平均盈利 = TotalProfit / ProfitVolume
        public double AverageLoss { get; private set; } // 平均亏损 = TotalLoss / LossVolume

        public double MaxDrawDown { get; private set; } // 最大回撤 =（前期高点-低点）
        public double MaxDrawDownRatio { get; private set; } // 最大回测比率 =（前期高点-低点）/前期高点
        public DateTime MaxDrawDownStartTime { get; private set; } // 最大回撤发生的起始时间
        public DateTime MaxDrawDownEndTime { get; private set;  } // 最大回撤发生的结束时间
        public double MaxDrawDownStartEquity { get; private set; } // 最大回撤发生的起始权益
        public double MaxDrawDownEndEquity { get; private set; } // 最大回撤发生的结束权益
        public double MAR { get; private set; } // = AnnualProfitRatio / MaxDrawDownRatio

        public double MaxProfitInOneTransaction { get; private set; } // 单次最大盈利
        public double MaxLossInOneTransaction { get; private set; } // 单词最大亏损
        public double ProfitRatioWithoutMaxProfit { get; private set; } // 扣除单次最大盈利后的收益率 = (NetProfit - MaxProfitInOneTransaction) / InitialEquity
        public double ProfitRatioWithoutMaxLoss { get; private set; }  // 扣除单词最大亏损后的收益率 = (NetProfit + MaxLossInOneTransaction) / InitialEquity

        public double StartPrice { get; private set; } // 品种起始价格 
        public double EndPrice { get; private set; } // 品种结束价格
        public double Rise { get; private set; } // 区间涨幅 = (EndPrice - StartPrice) / StartPrice 

        public IOrderedEnumerable<EquityPoint> EquitySequence { get; private set; } // 所有权益按周期排序
        public IOrderedEnumerable<CompletedTransaction> TransactionSequence { get; private set; } // all completed transactions, ordered by execution time and code

        public TradeMetric(
            string code,
            DateTime startDate,
            DateTime endDate,
            double startPrice,
            double endPrice,
            IOrderedEnumerable<EquityPoint> equitySequence, 
            IOrderedEnumerable<CompletedTransaction> transactionSequence)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
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

            if (equitySequence == null)
            {
                throw new ArgumentNullException("equitySequence");
            }

            if (transactionSequence == null)
            {
                throw new ArgumentNullException("transactionSequence");
            }

            if (equitySequence.Count() == 0)
            {
                throw new ArgumentException("equitySequence does not contain data");
            }

            if (equitySequence.First().Time < startDate)
            {
                throw new ArgumentOutOfRangeException("the smallest time in equitySequence is eariler than startDate");
            }

            if (equitySequence.Last().Time > endDate)
            {
                throw new ArgumentOutOfRangeException("the largest time in equitySequence is later than endDate");
            }

            Code = code;
            StartDate = startDate;
            EndDate = endDate;
            EquitySequence = equitySequence;
            TransactionSequence = transactionSequence;
            StartPrice = startPrice;
            EndPrice = endPrice;
            Rise = startPrice == 0.0 ? 0.0 : (endPrice - startPrice) / startPrice;

            Initialize();
        }

        private void Initialize()
        {
            // update trading days
            TotalTradingDays = (int)(EndDate.Subtract(StartDate).TotalDays + 1.0);

            // update initial/final equity
            InitialEquity = EquitySequence.First().Equity;
            FinalEquity = EquitySequence.Last().Equity;

            // update total/profit/loss periods and profit/loss period ratio
            TotalPeriods = EquitySequence.Count();
            ProfitPeriods = 0;
            LossPeriods = 0;
            double previousEquity = double.NaN;
            foreach (var e in EquitySequence)
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
            TotalProfit = TransactionSequence.Sum(ct => ct.SoldGain > ct.BuyCost ? ct.SoldGain - ct.BuyCost : 0.0);
            TotalLoss = TransactionSequence.Sum(ct => ct.SoldGain < ct.BuyCost ? ct.BuyCost - ct.SoldGain : 0.0);
            TotalCommission = TransactionSequence.Sum(ct => ct.Commission);
            NetProfit = TotalProfit - TotalLoss - TotalCommission;
            ProfitRatio = (NetProfit - InitialEquity) / InitialEquity;
            AnnualProfitRatio = ProfitRatio * 365 / TotalTradingDays;

            // update trading times related metrics
            TotalTradingTimes = TransactionSequence.Count();
            ProfitTradingTimes = TransactionSequence.Count(ct => ct.SoldGain > ct.BuyCost);
            LossTradingTimes = TransactionSequence.Count(ct => ct.SoldGain < ct.BuyCost);
            ProfitCoefficient = TotalTradingTimes == 0 ? 0.0 : (double)(ProfitTradingTimes - LossTradingTimes) / TotalTradingTimes;
            ProfitTimesRatio = TotalTradingTimes == 0 ? 0.0 : (double)ProfitTradingTimes / TotalTradingTimes;
            LossTimesRatio = TotalTradingTimes == 0 ? 0.0 : (double)LossTradingTimes / TotalTradingTimes;
            ProfitLossTimesRatio = LossTradingTimes == 0 ? 0.0 : (double)ProfitTradingTimes / LossTradingTimes;

            // update trading volume related metrics
            TotalVolume = TransactionSequence.Sum(ct => ct.Volume);
            ProfitVolume = TransactionSequence.Sum(ct => ct.SoldGain > ct.BuyCost ? ct.Volume : 0);
            LossVolume = TransactionSequence.Sum(ct => ct.SoldGain < ct.BuyCost ? ct.Volume : 0);
            AverageProfit = ProfitVolume == 0 ? 0.0 : TotalProfit / ProfitVolume;
            AverageLoss = LossVolume == 0 ? 0.0 : TotalLoss / LossVolume;

            // update max drawdown related metrics
            CalcuateMaxDrawDownMetrics();

            // update max profit/loss related metrics
            MaxProfitInOneTransaction = TransactionSequence.Max(ct => ct.SoldGain > ct.BuyCost ? ct.SoldGain - ct.BuyCost : 0.0);
            MaxLossInOneTransaction = TransactionSequence.Min(ct => ct.SoldGain < ct.BuyCost ? ct.BuyCost - ct.SoldGain : 0.0);
            ProfitRatioWithoutMaxProfit = (NetProfit - MaxProfitInOneTransaction) / InitialEquity;
            ProfitRatioWithoutMaxLoss = (NetProfit + MaxProfitInOneTransaction) / InitialEquity;
        }

        private void CalcuateMaxDrawDownMetrics()
        {
            EquityPoint[] equityPoints = EquitySequence.ToArray();
            
            MaxDrawDownRatio = double.MaxValue;
            bool foundMaxDrawDown = false;

            int i = 0;
            while (i < equityPoints.Length)
            {
                // find the first local peak
                double high = equityPoints[i].Equity;
                int highIndex = i;
                // find next high value
                for (int j = i + 1; j < equityPoints.Length; ++j)
                {
                    if (equityPoints[j].Equity < equityPoints[j - 1].Equity)
                    {
                        break;
                    }
                    else
                    {
                        high = equityPoints[j].Equity;
                        highIndex = j;
                    }
                }

                // find next high and the lowest value in between
                bool foundNextHigh = false;
                bool foundLowest = false;

                int nextHighIndex = highIndex;
                int lowestIndex = highIndex;
                double lowest = high;
                for (int j = highIndex + 1; j < equityPoints.Length; ++j)
                {
                    if (equityPoints[j].Equity > high)
                    {
                        foundNextHigh = true;
                        nextHighIndex = j;
                        break;
                    }
                    else if (equityPoints[j].Equity < lowest)
                    {
                        foundLowest = true;
                        lowest = equityPoints[j].Equity;
                        lowestIndex = j;
                    }
                }

                if (foundLowest)
                {
                    double drawDownRatio = (high - lowest) / high;
                    if (drawDownRatio > MaxDrawDownRatio)
                    {
                        MaxDrawDownRatio = drawDownRatio;
                        MaxDrawDown = high - lowest;
                        MaxDrawDownStartEquity = high;
                        MaxDrawDownStartTime = equityPoints[highIndex].Time;
                        MaxDrawDownEndEquity = lowest;
                        MaxDrawDownEndTime = equityPoints[lowestIndex].Time;

                        foundMaxDrawDown = true;
                    }
                }
                else
                {
                    if (foundNextHigh)
                    {
                        throw new InvalidOperationException("logic error");
                    }
                    else
                    {
                        // meet the end of sequence
                        break;                        
                    }
                }

                if (foundNextHigh)
                {
                    i = nextHighIndex;
                }
            }

            if (!foundMaxDrawDown)
            {
                MaxDrawDown = 0.0;
                MaxDrawDownRatio = 0.0;
                MaxDrawDownStartTime = EquitySequence.First().Time;
                MaxDrawDownStartEquity = InitialEquity;
                MaxDrawDownEndTime = EquitySequence.Last().Time;
                MaxDrawDownEndEquity = FinalEquity;
            }

            // update MAR
            MAR = MaxDrawDownRatio == 0.0 ? 0.0 : AnnualProfitRatio / MaxDrawDownRatio;
        }
    }
}
