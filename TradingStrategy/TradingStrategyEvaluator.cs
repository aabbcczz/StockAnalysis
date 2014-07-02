using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy
{
    public sealed class TradingStrategyEvaluator
    {
        private ITradingStrategy _strategy;
        private ITradingDataProvider _provider;
        private EquityManager _equityManager;
        private StandardTradingStrategyEvaluationContext _context;
        private TradingSettings _settings;
        private List<Transaction> _transactionHistory = new List<Transaction>();
        private Dictionary<string, ITradingObject> _indexedTradingObjects = null;

        private bool _evaluatable = true;

        public IEnumerable<Transaction> TransactionHistory
        {
            get { return _transactionHistory; }
        }

        public TradingStrategyEvaluator(ITradingStrategy strategy, ITradingDataProvider provider)
        {
            if (strategy == null || provider == null)
            {
                throw new ArgumentNullException();
            }

            _strategy = strategy;
            _provider = provider;
            
            _settings = _strategy.GetTradingSettings();

            _equityManager = new EquityManager(_strategy.GetInitialCapital(), _settings.SequenceOfSelling);
            _context = new StandardTradingStrategyEvaluationContext(_equityManager);
        }

        public void Evaluate()
        {
            if (!_evaluatable)
            {
                throw new InvalidOperationException("Evalute() can be called only once");
            }

            // initialize context
            _strategy.Initialize(_context);

            // Get all trading objects
            _indexedTradingObjects = _provider.GetAllTradingObjects().ToDictionary(t => t.Code);

            // warm up
            foreach (var code in _indexedTradingObjects.Keys)
            {
                ITradingObject tradingObject = _indexedTradingObjects[code];

                foreach (var bar in _provider.GetWarmUpData(code))
                {
                    _strategy.WarmUp(tradingObject, bar);
                }
            }

            // evaluating
            IDictionary<string, Bar> thisPeriodData = null;
            DateTime thisPeriodTime;

            IDictionary<string, Bar> nextPeriodData = null;
            DateTime nextPeriodTime;

            while ((thisPeriodData = _provider.GetNextPeriodData(out thisPeriodTime)) != null)
            {
                if (thisPeriodData.Count == 0)
                {
                    continue;
                }

                _strategy.StartPeriod(thisPeriodTime);
                
                foreach (var kvp in thisPeriodData)
                {
                    ITradingObject tradingObject = _indexedTradingObjects[kvp.Key];

                    if (kvp.Value.Time != thisPeriodTime)
                    {
                        throw new InvalidOperationException("Time in bar data is different with the time returned by data provider");
                    }

                    _strategy.Evaluate(tradingObject, kvp.Value);
                }

                var instructions = _strategy.EndPeriod();

                foreach(var instruction in instructions)
                {
                    Transaction transaction = BuildTransactionFromInstruction(instruction, thisPeriodTime);
                }
            }


            // finish evaluation
            _strategy.Finish();

            _evaluatable = false;
        }

        private Transaction BuildTransactionFromInstruction(Instruction instruction, DateTime time)
        {
            Transaction transaction = new Transaction()
            {
                Object = instruction.Object,
                Time = time,
            }
        }
    }
}
