using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class ProfitTraceStopLossMarketExiting 
        : GeneralTraceStopLossMarketExitingBase
    {
        private double[] _maxPercentageOfProfitDrawdown = null;

        [Parameter("50:25", "利润折回参数串。参数由冒号（:）分割的浮点数构成。按照顺序这些数代表利润为1到N个R（初始风险）时允许利润折回的最大百分比。连续的冒号表示值与前一个相同。")]
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
                    this.GetType().FullName,
                    string.Join(
                        ":", 
                        _maxPercentageOfProfitDrawdown
                            .Select(d => string.Format("0.00", d)))));
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice)
        {
            if (!Context.ExistsPosition(tradingObject.Code))
            {
                return 0.0;
            }

            var positions = Context.GetPositionDetails(tradingObject.Code);

            double totalVolume = positions.Sum(p => p.Volume);
            double totalProfit = positions.Sum(p => (currentPrice - p.BuyPrice) * p.Volume);
            double totalCost = positions.Sum(p => p.BuyPrice * p.Volume);
            double totalRisk = positions.Sum(p => p.InitialRisk);

            if (totalProfit <= 0.0)
            {
                return 0.0;
            }

            int index = (int)Math.Floor(totalProfit / totalRisk) - 1;
            if (index < 0)
            {
                return 0.0;
            }
            else if (index >= _maxPercentageOfProfitDrawdown.Length)
            {
                index = _maxPercentageOfProfitDrawdown.Length - 1;
            }

            double m = 1.0 - _maxPercentageOfProfitDrawdown[index] / 100.0;

            // calculate the price that can keep profit as m * totalProfit.
            // by simple induction, we know 
            //    new price = m * current price + (1 - m) * totalCost / totalVolume
            return m * currentPrice + (1.0 - m) * totalCost / totalVolume;
        }

        protected override void ValidateParameterValues()
        {
 	        base.ValidateParameterValues();

            if (string.IsNullOrEmpty(MaxPercentageOfProfitDrawdownString))
            {
                throw new ArgumentNullException();
            }

            string[] fields = MaxPercentageOfProfitDrawdownString.Split(new char[] { ':' });

            if (fields == null || fields.Length == 0)
            {
                throw new ArgumentException("Invalid parameter string");
            }

            List<double> parameters = new List<double>(fields.Length);
            double previousParameter = double.NaN;

            foreach (var field in fields)
            {
                double parameter;
                if (!double.TryParse(field, out parameter))
                {
                    if (double.IsNaN(previousParameter))
                    {
                        throw new ArgumentException("parameter is not numeric");
                    }
                    else
                    {
                        parameter = previousParameter;
                    }
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
