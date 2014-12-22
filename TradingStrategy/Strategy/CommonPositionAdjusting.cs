using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class CommonPositionAdjusting : GeneralPositionAdjustingBase
    {
        private Dictionary<string, double> _lastPositionInitialRisk = new Dictionary<string, double>();
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

        public override void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            _allTradingObjects = context.GetAllTradingObjects().ToDictionary(o => o.Code);
        }

        public override IEnumerable<Instruction> AdjustPositions()
        {
            var codes = Context.GetAllPositionCodes().ToArray();

            // remove all codes, which had been sold out, from stored last position risk 
            var codesToBeRemoved = _lastPositionInitialRisk.Keys.Except(codes).ToList();
            foreach (var code in codesToBeRemoved)
            {
                _lastPositionInitialRisk.Remove(code);
            }

            // add new codes in
            foreach (var code in codes)
            {
                if (!_lastPositionInitialRisk.ContainsKey(code))
                {
                    var position = Context.GetPositionDetails(code).OrderBy(p => p.BuyTime).Last();

                    if (position.IsStopLossPriceInitialized())
                    {
                        _lastPositionInitialRisk.Add(code, position.InitialRisk);
                    }
                }
            }

            List<Instruction> instructions = new List<Instruction>();

            // now check all codes if new position is required
            foreach (var kvp in _lastPositionInitialRisk)
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
                var tradingObject = _allTradingObjects[code];

                var bar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);

                if (bar.Time == Bar.InvalidTime)
                {
                    continue;
                }

                var gain = (bar.ClosePrice - lastPosition.BuyPrice) * lastPosition.Volume;
                if (gain > initialRisk * RiskPercentageTrigger / 100.0)
                {
                    instructions.Add(new Instruction()
                        {
                            Action = TradingAction.OpenLong,
                            Comments = string.Format(
                                "increase position. The gain of last position ({0:0.000}) exceeds initial risk ({1:0.000}) * {2:0.000}%",
                                gain,
                                initialRisk,
                                RiskPercentageTrigger),
                            SubmissionTime = bar.Time,
                            TradingObject = tradingObject,
                            Volume = lastPosition.Volume,
                            StopLossGapForBuying = -initialRisk / lastPosition.Volume
                        });
                }
            }

            instructions = instructions.OrderBy(instruction => instruction.TradingObject.Code).ToList();

            return instructions;
        }
    }
}
