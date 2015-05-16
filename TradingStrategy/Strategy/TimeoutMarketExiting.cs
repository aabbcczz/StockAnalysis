using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class TimeoutMarketExiting 
        : GeneralMarketExitingBase
    {
        public override string Name
        {
            get { return "定时退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有超过一段时间后立即退出市场"; }
        }

        [Parameter(20, "头寸持有周期数")]
        public int HoldingPeriods { get; set; }

        protected override void ValidateParameterValues()
        {
 	        base.ValidateParameterValues();

            if (HoldingPeriods < 0)
            {
                throw new ArgumentOutOfRangeException("HoldingPeriods must be great than 0");
            }
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            var code = tradingObject.Code;
            if (!Context.ExistsPosition(code))
            {
                return false;
            }

            int periodCount = Context.GetPositionDetails(code).Last().LastedPeriodCount;

            if (periodCount >= HoldingPeriods)
            {
                comments = string.Format("hold for {0} periods", HoldingPeriods);
                return true;
            }

            return false;
        }
    }
}
