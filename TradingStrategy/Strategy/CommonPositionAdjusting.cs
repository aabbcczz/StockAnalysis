﻿using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class CommonPositionAdjusting : GeneralPositionAdjustingBase
    {
        private Dictionary<string, double> _highestPrices = new Dictionary<string, double>(); 
        private Dictionary<string, double> _lastPositionInitialRisks = new Dictionary<string, double>();
        private Dictionary<string, ITradingObject> _allTradingObjects;

        public override string Name
        {
            get { return "通用头寸调整策略"; }
        }

        public override string Description
        {
            get { return "按照参数规定当盈利达到一定程度就加仓，直到最大仓位。"; }
        }

        [Parameter(4, "单交易对象最大允许的头寸个数（非总头寸数）")]
        public int MaxPositionCountOfEachObject { get; set; }

        [Parameter(50.0, 
@"加仓规则，由浮点数表示. 每当一个交易对象的最后一个头寸获利超过此头寸的初始风险*RiskPercentageTrigger/100, 则添加一个新头寸")]
        public double RiskPercentageTrigger { get; set; }

        [Parameter(false, "只在上升趋势加仓标志")]
        public bool AddPositionInUpTrendOnly { get; set; }

        [Parameter(10, "两个头寸所允许的最大间隔（按日计）， 0表示没有限制")]
        public int MaxIntervalInDaysBetweenTwoPositions { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (MaxPositionCountOfEachObject <= 0)
            {
                throw new ArgumentOutOfRangeException("MaxPositionCountFoEachObject must be greater than 0");
            }

            if (RiskPercentageTrigger <= 0.0)
            {
                throw new ArgumentOutOfRangeException("RiskPercentageTrigger must be greater than 0.0");
            }

            if(MaxIntervalInDaysBetweenTwoPositions < 0)
            {
                throw new ArgumentOutOfRangeException("MaxPeriodIntervalBetweenTwoPositions must be equal or greater than 0");
            }
        }

        public override void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            _allTradingObjects = context.GetAllTradingObjects().ToDictionary(o => o.Code);
        }

        public override void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            base.EvaluateSingleObject(tradingObject, bar);

            // record highest price.
            double highestPrice;
            if (_highestPrices.TryGetValue(tradingObject.Code, out highestPrice))
            {
                if (highestPrice < bar.ClosePrice)
                {
                    _highestPrices[tradingObject.Code] = bar.ClosePrice;
                }
            }
        }

        public override IEnumerable<Instruction> AdjustPositions()
        {
            var codes = Context.GetAllPositionCodes().ToArray();

            // remove all codes, which had been sold out, from stored last position risk and highest price.
            var codesToBeRemoved = _lastPositionInitialRisks.Keys.Except(codes).ToList();
            foreach (var code in codesToBeRemoved)
            {
                _lastPositionInitialRisks.Remove(code);
                _highestPrices.Remove(code);
            }

            // add new codes in
            foreach (var code in codes)
            {
                if (!_lastPositionInitialRisks.ContainsKey(code))
                {
                    var position = Context.GetPositionDetails(code).OrderBy(p => p.BuyTime).Last();

                    if (position.IsStopLossPriceInitialized())
                    {
                        _lastPositionInitialRisks.Add(code, position.InitialRisk);
                        _highestPrices.Add(code, position.BuyPrice);
                    }
                }
            }

            List<Instruction> instructions = new List<Instruction>();

            // now check all codes if new position is required
            foreach (var kvp in _lastPositionInitialRisks)
            {
                var code = kvp.Key;
                var initialRisk = kvp.Value;

                var positions = Context.GetPositionDetails(code).OrderBy(p => p.BuyTime);

                // do not exceed limit for each object.
                if (positions.Count() >= MaxPositionCountOfEachObject)
                {
                    continue;
                }

                var lastPosition = positions.Last();
                // ensure max period interval between two positions
                if (MaxIntervalInDaysBetweenTwoPositions > 0)
                {
                    var span = CurrentPeriod - lastPosition.BuyTime;
                    if (span.Days > MaxIntervalInDaysBetweenTwoPositions)
                    {
                        continue;
                    }
                }

                var tradingObject = _allTradingObjects[code];

                var bar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);

                if (bar.Time == Bar.InvalidTime)
                {
                    continue;
                }

                // ensure we increase position only in up trends
                if (AddPositionInUpTrendOnly && bar.ClosePrice < _highestPrices[code])
                {
                    continue;
                }

                var gain = (bar.ClosePrice - lastPosition.BuyPrice) * lastPosition.Volume;
                if (gain > initialRisk * RiskPercentageTrigger / 100.0)
                {
                    instructions.Add(
                        new OpenInstruction(bar.Time, tradingObject)
                        {
                            Comments = string.Format(
                                "increase position. The gain of last position ({0:0.000}) exceeds initial risk ({1:0.000}) * {2:0.000}%",
                                gain,
                                initialRisk,
                                RiskPercentageTrigger),
                            Volume = lastPosition.Volume,
                            StopLossGapForBuying = -initialRisk / lastPosition.Volume,
                            StopLossPriceForBuying = 0.0
                        });
                }
            }

            instructions = instructions.OrderBy(instruction => instruction.TradingObject.Code).ToList();

            return instructions;
        }
    }
}
