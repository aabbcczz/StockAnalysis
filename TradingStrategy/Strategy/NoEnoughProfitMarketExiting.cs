using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class NoEnoughProfitMarketExiting 
        : GeneralMarketExitingBase
    {

        private readonly Dictionary<int, int> _codesShouldExit = new Dictionary<int, int>();

        private int[] _holdingPeriods;

        public override string Name
        {
            get { return "限期未盈利退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有超过一段时间后若没有获得足够利润则退出市场"; }
        }

        [Parameter("3", "头寸持有周期数。多个周期用','分割")]
        public string HoldingPeriods { get; set; }

        [Parameter(0.0, "期待盈利百分比")]
        public double ExpectedProfitPercentage { get; set; }


        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            _holdingPeriods = HoldingPeriods.Split(new char[] { ',' }).Select(int.Parse).Where(i => i > 0).ToArray();

            if (_holdingPeriods.Any(i => i < 0))
            {
                throw new ArgumentOutOfRangeException("Holding periods can't be empty and must be great than 0");
            }

            Array.Sort(_holdingPeriods);
        }

        public override void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            if (_holdingPeriods.Length == 0)
            {
                return;
            }

            var code = tradingObject.Code;

            if (Context.ExistsPosition(code))
            {
                var temp = Context.GetPositionDetails(code);
                var positions = temp as Position[] ?? temp.ToArray();

                if (positions.Count() == 1) // possible newly created position
                {
                    // if lastestBuyTime == Period, it means the position is created at the morning, 
                    // so periodCount should be 0.
                    var holdingPeriod = positions.First().LastedPeriodCount;

                    foreach (var period in _holdingPeriods)
                    {
                        if (holdingPeriod == period)
                        {
                            if (positions.First().BuyPrice * (1.0 + ExpectedProfitPercentage / 100.0) >= bar.ClosePrice)
                            {
                                _codesShouldExit.Add(tradingObject.Index, period);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // for those code that has more than one position, this market exiting strategy 
                    // will not be used anyway
                    _codesShouldExit.Remove(tradingObject.Index);
                }
            }
            else
            {
                // remove all codes for non-existing positions
                _codesShouldExit.Remove(tradingObject.Index);
            }
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            if (_holdingPeriods.Length != 0)
            {
                int period;
                if (_codesShouldExit.TryGetValue(tradingObject.Index, out period))
                {
                    _codesShouldExit.Remove(tradingObject.Index);

                    result.Comments = string.Format("hold for {0} periods, but no profit", period);
                    result.ShouldExit = true;
                }
            }

            return result;
        }
    }
}
