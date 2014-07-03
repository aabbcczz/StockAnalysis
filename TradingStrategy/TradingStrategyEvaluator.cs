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
        private List<Instruction> _pendingInstructions = new List<Instruction>();

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
                RunPendingInstructions(thisPeriodData, thisPeriodTime, false);

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

                // get instructions and add them to pending instruction list
                var instructions = _strategy.GetInstructions();

                _pendingInstructions.AddRange(instructions);

                // run instructions
                RunPendingInstructions(thisPeriodData, thisPeriodTime, true);

                // end period
                _strategy.EndPeriod();
            }

            // finish evaluation
            _strategy.Finish();

            // Sell all equities forciably.


            // mark all pending instruction failed
            _pendingInstructions.Clear();

            // update 'evaluatable' flag to avoid this function be called twice
            _evaluatable = false;
        }

        private void RunPendingInstructions(
            IDictionary<string, Bar> tradingData, 
            DateTime time, 
            bool today)
        {
            // run instructions
            for (int i = 0; i < _pendingInstructions.Count; ++i)
            {
                var instruction = _pendingInstructions[i];

                TradingPriceOption option;
                if (instruction.Action == TradingAction.OpenLong)
                {
                    option = _settings.OpenLongPriceOption;
                }
                else if (instruction.Action == TradingAction.CloseLong)
                {
                    option = _settings.CloseLongPriceOption;
                }
                else
                {
                    throw new InvalidOperationException(
                        string.Format("unsupported action {0}", instruction.Action));
                }

                if (today)
                {
                    if (!option.HasFlag(TradingPriceOption.Today))
                    {
                        continue;
                    }
                }
                else
                {
                    if (option.HasFlag(TradingPriceOption.Today))
                    {
                        throw new InvalidProgramException("Logic error, all transaction expect to be executed in today should have been fully executed");
                    }
                }

                if (!tradingData.ContainsKey(instruction.Object.Code))
                {
                    if (today)
                    {
                        throw new InvalidOperationException(
                            string.Format("the trading object {0} can't be found in today trading data", instruction.Object.Code));
                    }
                    else
                    {
                        continue;
                    }
                }

                Transaction transaction = BuildTransactionFromInstruction(
                    instruction,
                    time,
                    tradingData[instruction.Object.Code]);

                string error;
                bool succeeded = _equityManager.ExecuteTransaction(transaction, out error);

                transaction.Succeeded = succeeded;
                transaction.Error = error;

                // notify transaction status
                _strategy.NotifyTransactionStatus(transaction);

                // add to history
                _transactionHistory.Add(transaction);

                // remove instruction that has been executed
                _pendingInstructions[i] = null;
            }

            // compact pending instruction list
            _pendingInstructions = _pendingInstructions.Where(i => i != null).ToList();
        }

        private Transaction BuildTransactionFromInstruction(Instruction instruction, DateTime time, Bar bar)
        {
            if (time != bar.Time)
            {
                throw new ArgumentException("Inconsistent time in bar and time parameters");
            }

            if ((instruction.Action == TradingAction.OpenLong 
                    && instruction.Volume % instruction.Object.VolumePerBuyingUnit != 0)
                || (instruction.Action == TradingAction.CloseLong
                    && instruction.Volume % instruction.Object.VolumePerSellingUnit != 0))
            {
                throw new InvalidOperationException("The volume of transaction does not meet trading object's requirement");
            }

            Transaction transaction = new Transaction()
            {
                Action = instruction.Action,
                Commission = 0.0,
                ExecutionTime = time,
                Object = instruction.Object,
                Price = CalculateTransactionPrice(bar, instruction),
                Succeeded = false,
                SubmissionTime = instruction.SubmissionTime,
                Volume = instruction.Volume
            };

            // update commission
            UpdateTransactionCommission(transaction);

            return transaction;
        }

        private double CalculateTransactionPrice(Bar bar, Instruction instruction)
        {
            double price;

            TradingPriceOption option;
            if (instruction.Action == TradingAction.OpenLong)
            {
                option = _settings.OpenLongPriceOption;
            }
            else if (instruction.Action == TradingAction.CloseLong)
            {
                option = _settings.CloseLongPriceOption;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("unsupported action {0}", instruction.Action));
            }

            if (option.HasFlag(TradingPriceOption.OpenPrice))
            {
                price = bar.OpenPrice;
            }
            else if (option.HasFlag(TradingPriceOption.ClosePrice))
            {
                price = bar.ClosePrice;
            }
            else if (option.HasFlag(TradingPriceOption.MiddlePrice))
            {
                price = (bar.OpenPrice + bar.ClosePrice + bar.HighestPrice + bar.LowestPrice ) / 4;
            }
            else if (option.HasFlag(TradingPriceOption.HighestPrice))
            {
                price = bar.HighestPrice;
            }
            else if (option.HasFlag(TradingPriceOption.LowestPrice))
            {
                price = bar.LowestPrice;
            }
            else
            {
                throw new InvalidProgramException("Logic error");
            }

            // count the spread now
            if (instruction.Action == TradingAction.OpenLong)
            {
                price += _settings.Spread * instruction.Object.MinPriceUnit;
            }
            else if (instruction.Action == TradingAction.CloseLong)
            {
                price -= _settings.Spread * instruction.Object.MinPriceUnit;
            }

            return price;
        }

        private void UpdateTransactionCommission(Transaction transaction)
        {
            Commission commission;

            if (transaction.Action == TradingAction.OpenLong)
            {
                commission = _settings.BuyingCommission;
            }
            else if (transaction.Action == TradingAction.CloseLong)
            {
                commission = _settings.SellingCommission;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("unsupported action {0}", transaction.Action));
            }

            if (commission.Type == Commission.CommissionType.ByAmount)
            {
                transaction.Commission = transaction.Price * transaction.Volume * commission.Tariff;
            }
            else if (commission.Type == Commission.CommissionType.ByVolume)
            {
                double hands = Math.Ceiling((double)transaction.Volume / transaction.Object.VolumePerHand);

                transaction.Commission = commission.Tariff * hands;
            }

            transaction.Commission = (double)((long)(transaction.Commission * 10000.0) / 10000L);
        }
    }
}
