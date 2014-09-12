using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class TimeoutMarketExiting 
        : GeneralMarketExitingBase
    {
        private Dictionary<string, int> _activePositionHoldingPeriods = new Dictionary<string, int>();
        private Dictionary<string, DateTime> _activePostionLatestBuyTime = new Dictionary<string, DateTime>();

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

            if (HoldingPeriods <= 0)
            {
                throw new ArgumentOutOfRangeException("HoldingPeriods must be great than 0");
            }
        }

        public override void Evaluate(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            string code = tradingObject.Code;
            if (Context.ExistsPosition(code))
            {
                DateTime latestBuyTime = Context.GetPositionDetails(code).Max(p => p.BuyTime);

                if (!_activePostionLatestBuyTime.ContainsKey(code))
                {
                    CreateNewRecord(code, latestBuyTime);
                }
                else
                {
                    if (latestBuyTime > _activePostionLatestBuyTime[code])
                    {
                        // new postion has been created, we need to reset record
                        int periodCount = latestBuyTime < Period ? 1 : 0;

                        _activePositionHoldingPeriods[code] = periodCount;
                    }
                    else
                    {
                        // just update period
                        _activePositionHoldingPeriods[code] = _activePositionHoldingPeriods[code] + 1;
                    }
                }
            }
            else
            {
                if (_activePositionHoldingPeriods.ContainsKey(code))
                {
                    _activePositionHoldingPeriods.Remove(code);
                    _activePostionLatestBuyTime.Remove(code);
                }
            }
        }

        private void CreateNewRecord(string code, DateTime latestBuyTime)
        {
            // create new record
            _activePostionLatestBuyTime.Add(code, latestBuyTime);
            int periodCount = latestBuyTime < Period ? 1 : 0;

            _activePositionHoldingPeriods.Add(code, periodCount);
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            int periodCount;

            if (_activePositionHoldingPeriods.TryGetValue(tradingObject.Code, out periodCount))
            {
                if (periodCount >= HoldingPeriods)
                {
                    comments = string.Format("hold for {0} periods", HoldingPeriods);
                    return true;
                }
            }

            return false;
        }
    }
}
