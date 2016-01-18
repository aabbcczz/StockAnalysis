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

        private readonly RuntimeMetricProxy _ma10;
        private readonly RuntimeMetricProxy _ma20;
        private readonly RuntimeMetricProxy _ma60;
        private readonly RuntimeMetricProxy _close;

        public BoardIndexBasedEquityUtilizationCalculator(IEvaluationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException();
            }

            _context = context;


            _ma10 = new RuntimeMetricProxy(_context.MetricManager, "MA[10]");
            _ma20 = new RuntimeMetricProxy(_context.MetricManager, "MA[20]");
            _ma60 = new RuntimeMetricProxy(_context.MetricManager, "MA[60]");
            _close = new RuntimeMetricProxy(_context.MetricManager, "BAR.CP");
        }

        public double CalculateEquityUtilization(ITradingObject tradingObject)
        {
            var boardIndexTradingObject = _context.GetBoardIndexTradingObject(tradingObject);
            if (boardIndexTradingObject == null)
            {
                return 1.0;
            }

            var ma10values = _ma10.GetMetricValues(boardIndexTradingObject);
            if (ma10values == null)
            {
                // the board index value is ready yet, back off to main board index
                boardIndexTradingObject = _context.GetBoardIndexTradingObject(StockBoard.MainBoard);
            }

            var closeValue = _close.GetMetricValues(boardIndexTradingObject)[0];
            var ma10Value = _ma10.GetMetricValues(boardIndexTradingObject)[0];
            var ma20Value = _ma20.GetMetricValues(boardIndexTradingObject)[0];
            var ma60Value = _ma60.GetMetricValues(boardIndexTradingObject)[0];

            if (closeValue >= ma10Value)
            {
                return 1.0;
            }
            else if (closeValue >= ma20Value)
            {
                return 0.7;
            }
            else if (closeValue >= ma60Value)
            {
                return 0.5;
            }
            else
            {
                return 0.3;
            }
        }
    }
}
