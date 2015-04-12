using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;
using StockAnalysis.Share;

namespace TradingStrategy.GroupMetrics
{
    public sealed class StockBoardMetricsManager : IGroupRuntimeMetricManagerObserver
    {
        private IEvaluationContext _context;

        private int[] _boardMetricIndices = new int[(int)StockBoard.All + 1];

        public delegate void AfterUpdatedMetricsDelegate();

        public AfterUpdatedMetricsDelegate AfterUpdatedMetrics
        {
            get; set;
        }

        public StockBoardMetricsManager(
            IEvaluationContext context, 
            Func<IEnumerable<ITradingObject>, IGroupRuntimeMetric> groupMetricCreator)
        {
            if (context == null || context.RelationshipManager == null || groupMetricCreator == null)
            {
                throw new ArgumentNullException();
            }

            _context = context;
            
            // create and register metric for blocks
            var allTradingObjects = context.GetAllTradingObjects();

            var metricPerBoard = Enumerable.Range(0, (int)StockBoard.All + 1)
                .Select(board =>
                    {
                        var tradingObjects = allTradingObjects
                            .Where(
                                o => 
                                {
                                    if (o.Object != null && o.Object is StockName)
                                    {
                                        StockName stockName = (StockName)o.Object;

                                        return (board & (int)stockName.Board) != 0;
                                    }

                                    return false;
                                })
                            .ToArray();

                        if (tradingObjects.Length == 0)
                        {
                            return null;
                        }
                        else
                        {
                            return groupMetricCreator(tradingObjects);
                        }
                    })
                .ToArray();

            _boardMetricIndices = metricPerBoard.Select(m => m == null ? -1 : context.GroupMetricManager.RegisterMetric(m)).ToArray();

            // register observer
            _context.GroupMetricManager.RegisterAfterUpdatedMetricsObserver(this);
        }

        public IGroupRuntimeMetric GetMetricForBoard(StockBoard board)
        {
            int index = _boardMetricIndices[(int)board];

            if (index < 0)
            {
                return null;
            }
            else
            {
                return _context.GroupMetricManager.GetMetric(index);
            }
        }

        public IGroupRuntimeMetric GetMetricForTradingObject(ITradingObject tradingObject)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException();
            }

            if (tradingObject.Object != null && tradingObject.Object is StockName)
            {
                StockName stockName = (StockName)tradingObject.Object;

                return GetMetricForBoard(stockName.Board);
            }
            else
            {
                return GetMetricForBoard(StockBoard.All);
            }
        }

        public void Observe(IGroupRuntimeMetricManager manager)
        {
            if (AfterUpdatedMetrics != null)
            {
                AfterUpdatedMetrics();
            }
        }
    }
}
