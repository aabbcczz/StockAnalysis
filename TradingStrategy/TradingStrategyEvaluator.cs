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

            while ((thisPeriodData = _provider.GetNextPeriodData(out thisPeriodTime)) != null)
            {
                if (thisPeriodData.Count == 0)
                {
                    continue;
                }
                
                // start a new period
                _strategy.StartPeriod(thisPeriodTime);
                
                // run pending instructions

                // notify transaction status

                // evaluate bar data
                foreach (var kvp in thisPeriodData)
                {
                    ITradingObject tradingObject = _indexedTradingObjects[kvp.Key];

                    if (kvp.Value.Time != thisPeriodTime)
                    {
                        throw new InvalidOperationException("Time in bar data is different with the time returned by data provider");
                    }

                    _strategy.Evaluate(tradingObject, kvp.Value);
                }

                // get instructions
                var instructions = _strategy.GetInstructions();

                // run instructions
                foreach(var instruction in instructions)
                {
                    Transaction transaction = BuildTransactionFromInstruction(instruction, thisPeriodTime);
                }

                // notify transaction status

                // end period
                _strategy.EndPeriod();
            }


            // finish evaluation
            _strategy.Finish();

            // Sell all equities forciably.

            // mark all pending transaction failed


            // update 'evaluatable' flag to avoid this function be called twice
            _evaluatable = false;
        }

        private Transaction BuildTransactionFromInstruction(Instruction instruction, DateTime time, Bar bar)
        {
            if (time != bar.Time)
            {
                throw new ArgumentException("Inconsistent time in bar and time parameters");
            }

            if (instruction.Action == TradingAction.Noop)
            {
                return null;
            }

            Transaction transaction = new Transaction()
            {
                Action = instruction.Action,
                Commission = 0.0,
                Object = instruction.Object,
                Price = 0.0,
                Succeeded = false,
                Time = time,
                Volume = instruction.Volume
            };

            if (transaction.Action == TradingAction.OpenLong)
            {
                // buy long
                if (_settings.BuyLongPriceOption == TradingPriceOption.)
            }
            return transaction;
        }

        private double CalculateTransactionPrice(Bar bar, TradingPriceOption option)
        {
            if (option)
        }
    }
}
