using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class ProfitTraceStopLossMarketExiting 
        : GeneralTraceStopLossMarketExitingBase
    {
        private double[] _maxPercentageOfProfitDrawdown;

        [Parameter("50:25", "利润折回参数串。参数由冒号（:）分割的浮点数构成。按照顺序这些数代表利润为0到N个R（初始风险）时允许利润折回的最大百分比。连续的冒号表示值与前一个相同。")]
        public string MaxPercentageOfProfitDrawdownString { get; set; }

        public override string Name
        {
            get { return "利润折回跟踪停价退市"; }
        }

        public override string Description
        {
            get { return "当利润超过一定程度后，从最高点损失一定百分比则触发停价退市"; }
        }

        public override void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            Context.Log(
                string.Format(
                    "{0} parameters: {1}",
                    GetType().FullName,
                    string.Join(
                        ":", 
                        _maxPercentageOfProfitDrawdown
                            .Select(d => string.Format("0.000", d)))));
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice, out string comments)
        {
            comments = string.Empty;

            if (!Context.ExistsPosition(tradingObject.Code))
            {
                return 0.0;
            }

            var temp = Context.GetPositionDetails(tradingObject.Code);
            var positions = temp as Position[] ?? temp.ToArray();

            double totalVolume = positions.Sum(p => p.Volume);
            var totalProfit = positions.Sum(p => (currentPrice - p.BuyPrice) * p.Volume);
            var totalCost = positions.Sum(p => p.BuyPrice * p.Volume);
            var totalRisk = positions.Sum(p => p.InitialRisk);

            if (totalProfit <= 0.0)
            {
                return 0.0;
            }

            var index = (int)Math.Floor(totalProfit / totalRisk) - 1;
            if (index < 0)
            {
                index = 0;
            }
            if (index >= _maxPercentageOfProfitDrawdown.Length)
            {
                index = _maxPercentageOfProfitDrawdown.Length - 1;
            }

            var m = 1.0 - _maxPercentageOfProfitDrawdown[index] / 100.0;

            // calculate the price that can keep profit as m * totalProfit.
            // by simple induction, we know 
            //    new price = m * current price + (1 - m) * totalCost / totalVolume
            var stoploss = m * currentPrice + (1.0 - m) * totalCost / totalVolume;
            comments = string.Format(
                "stoploss({0:0.000}) = m({1:0.000}) * Price({2:0.000}) + (1 - m) * totalCost({3:0.000}) / totalVolume({4:0.000})",
                stoploss,
                m,
                currentPrice,
                totalCost,
                totalVolume);

            return stoploss;
        }

        protected override void ValidateParameterValues()
        {
 	        base.ValidateParameterValues();

            if (string.IsNullOrEmpty(MaxPercentageOfProfitDrawdownString))
            {
                throw new ArgumentNullException();
            }

            var fields = MaxPercentageOfProfitDrawdownString.Split(new[] { ':' });

            if (fields == null || fields.Length == 0)
            {
                throw new ArgumentException("Invalid parameter string");
            }

            var parameters = new List<double>(fields.Length);
            var previousParameter = double.NaN;

            foreach (var field in fields)
            {
                double parameter;
                if (!double.TryParse(field, out parameter))
                {
                    if (double.IsNaN(previousParameter))
                    {
                        throw new ArgumentException("parameter is not numeric");
                    }
                    parameter = previousParameter;
                }

                if (parameter <= 0.0 || parameter > 100.0)
                {
                    throw new ArgumentOutOfRangeException("parameter is not in (0.0, 100.0]");
                }

                parameters.Add(parameter);

                // save current parameter value to 'previousParameter'
                previousParameter = parameter;
            }

            _maxPercentageOfProfitDrawdown = parameters.ToArray();
        }
    }
}
