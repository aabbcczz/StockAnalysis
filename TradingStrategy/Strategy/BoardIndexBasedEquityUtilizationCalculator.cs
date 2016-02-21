using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    sealed class BoardIndexBasedEquityUtilizationCalculator
    {
        private readonly IEvaluationContext _context;

        private readonly RuntimeMetricProxy _ma;
        private readonly RuntimeMetricProxy _close;

        private const int MinPercentage = 90;
        private const int MaxPercentage = 110;

        // 90..110
        private double[] _utilizations = new double[]
        {
            0.10,
            0.22,
            0.25,
            0.10,
            0.29,
            0.16,
            0.16,
            0.36,
            0.12,
            0.31,
            0.43,
            0.36,
            0.39,
            0.37,
            0.31,
            0.48,
            0.19,
            0.49,
            0.79,
            0.25,
            0.27,
        };

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

        public double CalculateEquityUtilizationPerTradingObject(ITradingObject tradingObject)
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

            var percentage = (int)(closeValue * 100.0 / maValue);

            if (percentage < MinPercentage || percentage > MaxPercentage)
            {
                return 0.0;
            }

            return _utilizations[percentage - MinPercentage];
        }
    }
}
