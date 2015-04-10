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
        private readonly Dictionary<string, int> _activePositionHoldingPeriods = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _codesShouldExit = new Dictionary<string, int>();

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

            // remove all codes for non-existing positions
            if (!Context.ExistsPosition(code))
            {
                RemoveRecord(code);
            }
            else
            {
                var temp = Context.GetPositionDetails(code);
                var positions = temp as Position[] ?? temp.ToArray();

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

                    var holdingPeriod = _activePositionHoldingPeriods[code];
                    foreach (var period in _holdingPeriods)
                    {
                        if (holdingPeriod == period)
                        {
                            if (positions.First().BuyPrice * (1.0 + ExpectedProfitPercentage / 100.0) >= bar.ClosePrice)
                            {
                                _codesShouldExit.Add(code, period);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // for those code that has more than one position, this market exiting strategy 
                    // will not be used anyway
                    RemoveRecord(code);
                }
            }
        }

        private void CreateNewRecord(string code, DateTime latestBuyTime)
        {
            // create new record

            // if lastestBuyTime == Period, it means the position is created at the morning, 
            // so periodCount should be 0.
            var periodCount = latestBuyTime < CurrentPeriod ? 1 : 0;

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

            if (_holdingPeriods.Length == 0)
            {
                return false;
            }

            int period;
            if (_codesShouldExit.TryGetValue(tradingObject.Code, out period))
            {
                _codesShouldExit.Remove(tradingObject.Code);

                comments = string.Format("hold for {0} periods, but no profit", period);
                return true;
            }

            return false;
        }
    }
}
