using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class TradeCounter
    {
        public string Code { get; set; } // if Code is string.Empty, it represents all trading objects.
        public DateTime StartDate { get; set; } // 统计起始日期
        public DateTime EndDate { get; set; }  // 统计结束日期
        public int TotalTradingDays { get; set; } // 总交易天数 = EndDate - StartDate + 1 { get; set; }

        public double InitialEquity { get; set; } // 初始权益
        public double FinalEquity { get; set; } // 期末权益

        public int TotalPeriods { get; set; } // 总周期数
        public int ProfitPeriods { get; set; } // 盈利周期数, 即本周期权益比上一周期上升
        public int LossPeriods { get; set; } // 亏损周期数，即本周期权益比上一周期下降
        public double ProfitLossPeriodRatio { get; set; } // 盈亏周期比，=ProfitPeriods/LossPeriods { get; set; }

        public double TotalProfit { get; set; } // 总盈利
        public double TotalLoss { get; set; } // 总亏损
        public double TotalCommission { get; set; } // 总手续费
        public double NetProfit { get; set; } // 净利润 = TotalProfit - TotalLoss - TotalCommission
        public double ProfitRatio { get; set; } // 收益率 = (NetProfit - InitialEquity) / InitialEquity { get; set; }
        public double AnnualProfitRatio { get; set; } // 年化收益率 = ProfitRatio / TotalTradingDays * 365

        public int TotalTradingTimes { get; set; } // 总交易次数，每次卖出算做一次交易 = ProfitTradingTimes + LossTradingTimes
        public int ProfitTradingTimes { get; set; } // 盈利交易次数
        public int LossTradingTimes { get; set; } // 亏损交易次数
        public double ProfitCoefficient { get; set; } // 盈利系数 = (ProfitTradingTimes - LossTradingTimes) / TotalTradingTimes { get; set; }
        public double ProfitTimesRatio { get; set; } // 胜率 = ProfitTradingTimes / TradingTimes { get; set; }
        public double LossTimesRatio { get; set; } // 亏损比率 = LossTradingTimes / TotalTradingTimes { get; set; }
        public double ProfitLossTimesRatio { get; set; } // 盈亏次数比 = ProfitTradingTimes / LossTradingTimes { get; set; }

        public int TotalVolume { get; set; } // 总交易量 = ProfitVolume + LossVolume
        public int ProfitVolume { get; set; } // 盈利量, 所有盈利交易中交易量的总和
        public int LossVolume { get; set; } // 亏损量，所以亏损交易中交易量的总和
        public double AverageProfit { get; set; } // 平均盈利 = TotalProfit / ProfitVolume
        public double AverageLoss { get; set; } // 平均亏损 = TotalLoss / LossVolume

        public double MaxDrawDown { get; set; } // 最大回撤 =（前期高点-低点）
        public double MaxDrawDownRatio { get; set; } // 最大回测比率 =（前期高点-低点）/前期高点

        public double StartPrice { get; set; } // 品种起始价格 
        public double EndPrice { get; set; } // 品种结束价格
        public double Rise { get; set; } // 区间涨幅 = (EndPrice - StartPrice) / StartPrice 

        public double MAR { get; set; } // = AnnualProfitRatio / MaxDrawDownRatio

        public double MaxProfitInOneTransaction { get; set; } // 单次最大盈利
        public double MaxLossInOneTransaction { get; set; } // 单词最大亏损
        public double ProfitRatioWithoutMaxProfit { get; set; } // 扣除单次最大盈利后的收益率 = (NetProfit - MaxProfitInOneTransaction) / InitialEquity
        public double ProfitRatioWithoutMaxLoss { get; set; }  // 扣除单词最大亏损后的收益率 = (NetProfit + MaxLossInOneTransaction) / InitialEquity

        public IEnumerable<DateTime> Periods { get; set; } // 所有周期，已排序
        public IEnumerable<double> EquityPerPeriod { get; set; } // 所有权益（按日期）
        public IEnumerable<CompletedTransaction> Transactions { get; set; } // all completed transactions
    }
}
