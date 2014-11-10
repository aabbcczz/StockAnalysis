using System;
using System.Collections.Generic;
using TradingStrategy;
using StockAnalysis.Share;

namespace TradingStrategyEvaluation
{
    internal sealed class StandardEvaluationContext : IEvaluationContext
    {
        private Bar[] _currentPeriodData;
        private readonly EquityManager _equityManager;
        private readonly ILogger _logger;
        private readonly ITradingDataProvider _provider;
        private readonly IRuntimeMetricManager _metricManager;

        public IRuntimeMetricManager MetricManager
        {
            get { return _metricManager; }
        }

        public StandardEvaluationContext(
            ITradingDataProvider provider, 
            EquityManager manager, 
            ILogger logger)
        {
            if (manager == null || provider == null || logger == null)
            {
                throw new ArgumentNullException();
            }

            _provider = provider;
            _equityManager = manager;
            _logger = logger;

            _metricManager = new StandardRuntimeMetricManager(_provider.GetAllTradingObjects().Length);
        }

        public double GetInitialEquity()
        {
            return _equityManager.InitialCapital;
        }

        public double GetCurrentEquity(DateTime period, EquityEvaluationMethod method)
        {
            return _equityManager.GetTotalEquity(_provider, period, method);
        }

        public IEnumerable<ITradingObject> GetAllTradingObjects()
        {
            return _provider.GetAllTradingObjects();
        }

        public int GetCountOfTradingObjects()
        {
            return _provider.GetAllTradingObjects().Length;
        }

        public IEnumerable<string> GetAllPositionCodes()
        {
            return _equityManager.GetAllPositionCodes();
        }

        public bool ExistsPosition(string code)
        {
            return _equityManager.ExistsPosition(code);
        }

        public IEnumerable<Position> GetPositionDetails(string code)
        {
            return _equityManager.GetPositionDetails(code);
        }

        public Bar GetBarOfTradingObjectForCurrentPeriod(ITradingObject tradingObject)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException();
            }

            if (_currentPeriodData == null)
            {
                throw new InvalidOperationException("There is no data for current period");
            }

            return _currentPeriodData[tradingObject.Index];
        }

        public void SetCurrentPeriodData(Bar[] data)
        {
            _currentPeriodData = data;
        }

        public void Log(string log)
        {
            if (_logger != null)
            {
                _logger.Log(log);
            }
        }
    }
}
