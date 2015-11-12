using System;
using System.Collections.Generic;

using MetricsDefinition;
using TradingStrategy.Base;
using TradingStrategy.MetricBooleanExpression;
using TradingStrategy.GroupMetrics;

namespace TradingStrategy.Strategy
{
    public sealed class WeekdayMarketEntering
        : GeneralMarketEnteringBase
    {
        public override string Name
        {
            get { return "Weekday入市"; }
        }

        public override string Description
        {
            get { return "当交易日期是指定的weekday入市"; }
        }

        [Parameter(0x7F, "允许交易日期的掩码，最低7位分别代表星期日到星期六是否允许入市。当掩码位为1时可入市")]
        public int WeekdayMask { get; set; }

        public override MarketEnteringComponentResult CanEnter(ITradingObject tradingObject)
        {
            var result = new MarketEnteringComponentResult();

            if ((WeekdayMask & (1 << (int)CurrentPeriod.DayOfWeek)) != 0)
            {
                result.Comments = CurrentPeriod.DayOfWeek.ToString();
                result.CanEnter = true;
            }

            return result;
        }
    }
}
