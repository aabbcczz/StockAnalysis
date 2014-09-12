using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class NoEnoughProfitMarketExiting 
        : GeneralMarketExitingBase
    {
        private Dictionary<string, int> _activePositionHoldingPeriods = new Dictionary<string, int>();
        private HashSet<string> _codesShouldExit = new HashSet<string>();

        public override string Name
        {
            get { return "限期未盈利退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有超过一段时间后若没有获得足够利润则退出市场"; }
        }

        [Parameter(3, "头寸持有周期数")]
        public int HoldingPeriods { get; set; }

        [Parameter(0.0, "期待盈利百分比")]
        public double ExpectedProfitPercentage { get; set; }


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

            // remove all codes for non-existing positions
            if (!Context.ExistsPosition(code))
            {
                RemoveRecord(code);
            }
            else
            {
                var positions = Context.GetPositionDetails(code);
                if (positions.Count() == 1) // possible newly created position
                {
                    if (!_activePositionHoldingPeriods.ContainsKey(code))
                    {
                        CreateNewRecord(code, positions.First().BuyTime);
                    }
                    else
                    {
                        _activePositionHoldingPeriods[code] = _activePositionHoldingPeriods[code] + 1;
                    }

                    if (_activePositionHoldingPeriods[code] == HoldingPeriods)
                    {
                        if (positions.First().BuyPrice * (1.0 + ExpectedProfitPercentage / 100.0) >= bar.ClosePrice)
                        {
                            _codesShouldExit.Add(code);
                        }
                    }
                }
                else
                {
                    // for those code that has more than one positions, this market exiting strategy 
                    // will not be used anyway
                    RemoveRecord(code);
                }
            }
        }

        private void CreateNewRecord(string code, DateTime latestBuyTime)
        {
            // create new record
            int periodCount = latestBuyTime < Period ? 1 : 0;

            _activePositionHoldingPeriods.Add(code, periodCount);
        }

        private void RemoveRecord(string code)
        {
            _activePositionHoldingPeriods.Remove(code);
            _codesShouldExit.Remove(code);
        }

        public override void Finish()
        {
            _activePositionHoldingPeriods.Clear();
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            if (_codesShouldExit.Contains(tradingObject.Code))
            {
                comments = string.Format("hold for {0} periods, but no profit", HoldingPeriods);
                return true;
            }

            return false;
        }
    }
}
