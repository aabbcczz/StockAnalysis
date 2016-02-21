using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy.Base;
using TradingStrategy.MetricBooleanExpression;

namespace TradingStrategy.Strategy
{
    public sealed class SingleMovingAverageBoardIndexMarketEntering
        : BoardIndexMetricBasedMarketEntering
    {
        [Parameter("AMA", "移动平均指标名")]
        public string MovingAverageMetricName { get; set; }

        [Parameter(10, "移动平均周期")]
        public int MovingAveragePeriod { get; set; }

        [Parameter(-1, "参数1, 小于0将被忽略")]
        public int Argument1 { get; set; }

        [Parameter(-1, "参数2, 小于0将被忽略")]
        public int Argument2 { get; set; }

        [Parameter(1, "触发条件。1表示收盘价高于移动平均值触发, 0表示收盘价低于移动平均值触发")]
        public int TriggeringCondition { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (MovingAveragePeriod <= 0)
            {
                throw new ArgumentException("Period value can't be smaller than 0");
            }

            if (TriggeringCondition != 0 && TriggeringCondition != 1)
            {
                throw new ArgumentException("TriggeringCondition must be 0 or 1");
            }
        }

        protected override MetricBooleanExpression.IMetricBooleanExpression BuildExpression()
        {
            string expression;

            if (Argument2 < 0)
            {
                if (Argument1 < 0)
                {
                    expression = 
                        string.Format(
                            "BAR.CP {0} {1}[{2}]",
                            TriggeringCondition == 0 ? '<' : '>',
                            MovingAverageMetricName,
                            MovingAveragePeriod);

                }
                else
                {
                    expression =
                        string.Format(
                            "BAR.CP {0} {1}[{2},{3}]",
                            TriggeringCondition == 0 ? '<' : '>',
                            MovingAverageMetricName,
                            MovingAveragePeriod,
                            Argument1);

                }
            }
            else
            {
                expression =
                    string.Format(
                        "BAR.CP {0} {1}[{2},{3},{4}]",
                        TriggeringCondition == 0 ? '<' : '>',
                        MovingAverageMetricName,
                        MovingAveragePeriod,
                        Argument1,
                        Argument2);

            }

            return new Comparison(expression);
        }

        public override string Name
        {
            get { return "单移动平均板块指数入市"; }
        }

        public override string Description
        {
            get { return "当板块指数收盘价和移动平均值满足触发条件时允许入市"; }
        }
    }
}
