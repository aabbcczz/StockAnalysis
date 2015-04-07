using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class FirstBailoutMarketExiting 
        : GeneralMarketExitingBase
    {
        private readonly Dictionary<int, int> _activeKeptPeriods = new Dictionary<int, int>();
        private readonly Dictionary<int, string> _comments = new Dictionary<int, string>();

        public override string Name
        {
            get { return "首次获利退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有首次获利后退出市场"; }
        }

        [Parameter(0, "价格选择选项。0为最高价，1为最低价，2为收盘价，3为开盘价")]
        public int PriceSelector { get; set; }

        [Parameter(0, "获利后保持周期数")]
        public int KeepPeriods { get; set; }

        protected override void ValidateParameterValues()
        {
 	        base.ValidateParameterValues();

            if (!BarPriceSelector.IsValidSelector(PriceSelector))
            {
                throw new ArgumentException("价格选择项非法");
            }

            if (KeepPeriods < 0)
            {
                throw new ArgumentException("获利后保持周期数非法");
            }
        }

        public override void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            base.EvaluateSingleObject(tradingObject, bar);

            if (_activeKeptPeriods.ContainsKey(tradingObject.Index))
            {
                _activeKeptPeriods[tradingObject.Index] += 1;
            }
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            if(!Context.ExistsPosition(tradingObject.Code))
            {
                return false;
            }

            if (_activeKeptPeriods.ContainsKey(tradingObject.Index))
            {
                if (_activeKeptPeriods[tradingObject.Index] >= KeepPeriods)
                {
                    comments = _comments[tradingObject.Index];

                    _activeKeptPeriods.Remove(tradingObject.Index);
                    _comments.Remove(tradingObject.Index);

                    return true;
                }
            }
            else
            {
                var position = Context.GetPositionDetails(tradingObject.Code).First();
                if (CurrentPeriod <= position.BuyTime)
                {
                    return false;
                }

                var bar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);
                var price = BarPriceSelector.Select(bar, PriceSelector);

                if (position.BuyPrice < price)
                {
                    string tempComments = string.Format("Bailout: buy price {0:0.000}, current price {1:0.000}", position.BuyPrice, price);

                    if (KeepPeriods > 0)
                    {
                        _comments.Add(tradingObject.Index, tempComments);
                        _activeKeptPeriods.Add(tradingObject.Index, 0);

                        return false;
                    }
                    else
                    {
                        comments = tempComments;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
