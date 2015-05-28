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
        private readonly IGroupRuntimeMetricManager _groupMetricManager;
        private readonly StockBlockRelationshipManager _relationshipManager;
        private readonly IDataDumper _dumper;
        private readonly TradingSettings _settings = null;

        public IRuntimeMetricManager MetricManager
        {
            get { return _metricManager; }
        }

        public IGroupRuntimeMetricManager GroupMetricManager
        {
            get { return _groupMetricManager; }
        }

        public StockBlockRelationshipManager RelationshipManager
        {
            get { return _relationshipManager; }
        }

        public StandardEvaluationContext(
            ITradingDataProvider provider, 
            EquityManager equityManager, 
            ILogger logger,
            TradingSettings settings = null,
            IDataDumper dumper = null,
            StockBlockRelationshipManager relationshipManager  = null)
        {
            if (equityManager == null || provider == null || logger == null)
            {
                throw new ArgumentNullException();
            }

            _provider = provider;
            _equityManager = equityManager;
            _logger = logger;
            _settings = settings;
            _dumper = dumper;
            _relationshipManager = relationshipManager;

            var metricManager = new StandardRuntimeMetricManager(_provider.GetAllTradingObjects().Length);
            var groupMetricManager = new StandardGroupRuntimeMetricManager(metricManager);

            // register the group metric manager as observer of metric manager.
            metricManager.RegisterAfterUpdatedMetricsObserver(groupMetricManager);

            _metricManager = metricManager;
            _groupMetricManager = groupMetricManager;
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

        public ITradingObject GetTradingObject(string code)
        {
            int index = _provider.GetIndexOfTradingObject(code);
            if (index < 0)
            {
                throw new IndexOutOfRangeException("can't find trading object for code " + code);
            }

            return _provider.GetAllTradingObjects()[index];
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

        public void DumpBarsFromCurrentPeriod(ITradingObject tradingObject)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException();
            }

            if (_dumper != null)
            {
                var bars = _provider.GetAllBarsForTradingObject(tradingObject.Index);
                var currentBar = GetBarOfTradingObjectForCurrentPeriod(tradingObject);

                int index = FindIndexOfBar(bars, currentBar);
                if (index < 0)
                {
                    throw new InvalidOperationException("Logic error");
                }

                _dumper.Dump(bars, index);
            }
        }

        private int FindIndexOfBar(Bar[] bars, Bar bar)
        {
            int index = Array.BinarySearch(bars, bar, new Bar.TimeComparer());
            return index < 0 ? -1 : index;
        }

        public void SetDefaultPriceForInstructionWhenNecessary(Instruction instruction)
        {
            if (_settings == null)
            {
                return;
            }

            if (instruction.Price == null)
            {
                TradingPrice price;

                if (instruction.Action == TradingAction.OpenLong)
                {
                    price = new TradingPrice(_settings.OpenLongPricePeriod, _settings.OpenLongPriceOption, 0.0);
                }
                else if (instruction.Action == TradingAction.CloseLong)
                {
                    price = new TradingPrice(_settings.CloseLongPricePeriod, _settings.CloseLongPriceOption, 0.0);
                }
                else
                {
                    throw new InvalidOperationException(
                        string.Format("unsupported action {0}", instruction.Action));
                }

                instruction.Price = price;
            }
        }
    }
}
