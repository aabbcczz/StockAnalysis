namespace StockAnalysis.TradingStrategy
{
    using System;
    using Common.ChineseMarket;

    public sealed class UnifiedMetricProxy
    {
        private const string ForBoardIndexMetricHeader = "B_";

        private readonly IEvaluationContext _context;

        public RuntimeMetricProxy Proxy
        {
            get;
            private set;
        }

        public bool IsBoardIndexMetric
        {
            get;
            private set;
        }

        public string RealMetricName
        {
            get;
            private set;
        }

        public UnifiedMetricProxy(string metric, IEvaluationContext context)
        {
            if (string.IsNullOrWhiteSpace(metric) || context == null)
            {
                throw new ArgumentNullException();
            }

            _context = context;

            if (metric.StartsWith(ForBoardIndexMetricHeader))
            {
                // for board
                IsBoardIndexMetric = true;
                RealMetricName = metric.Substring(ForBoardIndexMetricHeader.Length);
            }
            else
            {
                IsBoardIndexMetric = true;
                RealMetricName = metric;
            }

            Proxy = new RuntimeMetricProxy(_context.MetricManager, RealMetricName);
        }

        public double[] GetValues(ITradingObject tradingObject)
        {
            ITradingObject trueObject = IsBoardIndexMetric ? _context.GetBoardIndexTradingObject(tradingObject) : tradingObject;

            var values = Proxy.GetMetricValues(trueObject);
            if (values == null)
            {
                if (!object.ReferenceEquals(trueObject, tradingObject))
                {
                    trueObject = _context.GetBoardIndexTradingObject(StockBoard.MainBoard);
                    values = Proxy.GetMetricValues(trueObject);
                }
            }

            return values;
        }

        public double GetValue(ITradingObject tradingObject)
        {
            ITradingObject trueObject = IsBoardIndexMetric ? _context.GetBoardIndexTradingObject(tradingObject) : tradingObject;

            var values = Proxy.GetMetricValues(trueObject);
            if (values == null)
            {
                if (!object.ReferenceEquals(trueObject, tradingObject))
                {
                    trueObject = _context.GetBoardIndexTradingObject(StockBoard.MainBoard);
                    values = Proxy.GetMetricValues(trueObject);
                }
            }

            return values == null ? 0.0 : values[0];
        }
    }
}
