using System;
using StockAnalysis.Common.ChineseMarket;

namespace StockAnalysis.TradingStrategy.Strategy
{
    sealed class BoardIndexBasedEquityUtilizationCalculator
    {
        private readonly IEvaluationContext _context;

        private readonly RuntimeMetricProxy _ma;
        private readonly RuntimeMetricProxy _close;

        private static double _reciprocalSqrt2Pi = 1.0 / Math.Sqrt(2 * Math.PI);
        private double _normalDistribution0 = NormalDistribution(0, 1.0, 0);

        public BoardIndexBasedEquityUtilizationCalculator(IEvaluationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException();
            }

            _context = context;

            _ma = new RuntimeMetricProxy(_context.MetricManager, "AMA[22]");
            _close = new RuntimeMetricProxy(_context.MetricManager, "BAR.CP");
        }

        private static double NormalDistribution(double mu, double sigma, double x)
        {
            double u = (x - mu) / sigma;

            return _reciprocalSqrt2Pi * Math.Exp(-u * u / 2.0);
        }

        public double CalculateEquityUtilization(ITradingObject tradingObject)
        {
            var boardIndexTradingObject = _context.GetBoardIndexTradingObject(tradingObject);
            if (boardIndexTradingObject == null)
            {
                return 1.0;
            }

            var maValues = _ma.GetMetricValues(boardIndexTradingObject);
            if (maValues == null)
            {
                // the board index value is not ready yet, back off to main board index
                boardIndexTradingObject = _context.GetBoardIndexTradingObject(StockBoard.MainBoard);
            }

            var closeValue = _close.GetMetricValues(boardIndexTradingObject)[0];
            var maValue = _ma.GetMetricValues(boardIndexTradingObject)[0];

            var percentage = closeValue / maValue;

            double utilization;

            //if (percentage > 1.1)
            //{
            //    utilization = 1.0 - (percentage - 1.1) * 2.0;
            //}
            //else if (percentage < 0.9)
            //{
            //    utilization = 1.0;
            //}
            //else
            //{
            //    utilization = 0.7;
            //}

            utilization = 1.0;
            return Math.Max(Math.Min(utilization, 1.0), 0.1);

            //var utilization = NormalDistribution(1.0, 0.1, percentage) / _normalDistribution0;

            //return Math.Max(utilization, 0.1);
        }
    }
}
