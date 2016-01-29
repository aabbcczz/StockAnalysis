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

        private readonly RuntimeMetricProxy _ma5;
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

            _ma5 = new RuntimeMetricProxy(_context.MetricManager, "AMA[5]");
            _ma10 = new RuntimeMetricProxy(_context.MetricManager, "AMA[10]");
            _ma20 = new RuntimeMetricProxy(_context.MetricManager, "AMA[20]");
            _ma60 = new RuntimeMetricProxy(_context.MetricManager, "AMA[60]");
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
            var ma5Value = _ma5.GetMetricValues(boardIndexTradingObject)[0];
            var ma10Value = _ma10.GetMetricValues(boardIndexTradingObject)[0];
            var ma20Value = _ma20.GetMetricValues(boardIndexTradingObject)[0];
            var ma60Value = _ma60.GetMetricValues(boardIndexTradingObject)[0];

            if (ma5Value < ma20Value)
            {
                // descending trends
                if (closeValue < ma5Value)
                {
                    return 0.0;
                }
                else if (closeValue >= ma5Value && closeValue < ma20Value)
                {
                    return 0.15;
                }
                else if (closeValue >= ma20Value)
                {
                    return 0.3;
                }
            }
            else
            {
                // ascending trends
                if (closeValue >= ma5Value)
                {
                    return 1.0;
                }
                else if (closeValue >= ma20Value && closeValue < ma5Value)
                {
                    return 0.7;
                }
                else if (closeValue < ma20Value)
                {
                    return 0.5;
                }
            }

            return 0.0;
        }
    }
}
