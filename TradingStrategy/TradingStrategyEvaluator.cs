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
        private ITradingObject[] _allTradingObjects = null;
        private Dictionary<string, int> _tradingObjectIndexByCode = new Dictionary<string,int>();
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
            _allTradingObjects = _provider.GetAllTradingObjects().ToArray();
            for (int i = 0; i < _allTradingObjects.Length; ++i)
            {
                _tradingObjectIndexByCode.Add(_allTradingObjects[i].Code, i);
            }

            // warm up
            foreach (var tradingObject in _allTradingObjects)
            {
                var warmupData = _provider.GetWarmUpData(tradingObject.Code);
                if (warmupData != null)
                {
                    foreach (var bar in _provider.GetWarmUpData(tradingObject.Code))
                    {
                        _strategy.WarmUp(tradingObject, bar);
                    }
                }
            }

            // evaluating
            Bar[] thisPeriodData = null;
            DateTime thisPeriodTime;
            DateTime lastPeriodTime = DateTime.MinValue;

            while ((thisPeriodData = _provider.GetNextPeriodData(out thisPeriodTime)) != null)
            {
                if (thisPeriodData.Length != _allTradingObjects.Length)
                {
                    throw new InvalidOperationException("data length does not equal to trading object number");
                }
                
                // start a new period
                _strategy.StartPeriod(thisPeriodTime);
                
                // run pending instructions left over from previous period
                RunPendingInstructions(thisPeriodData, thisPeriodTime, false);

                // evaluate bar data
                for (int i = 0; i < thisPeriodData.Length; ++i)
                {
                    Bar bar = thisPeriodData[i];
                    ITradingObject tradingObject = _allTradingObjects[i];

                    if (!bar.Invalid())
                    {
                        if (bar.Time != thisPeriodTime)
                        {
                            throw new InvalidOperationException("Time in bar data is different with the time returned by data provider");
                        }

                        _strategy.Evaluate(tradingObject, bar);
                    }
                }

                // get instructions and add them to pending instruction list
                var instructions = _strategy.GetInstructions();

                _pendingInstructions.AddRange(instructions);

                // run instructions for current period
                RunPendingInstructions(thisPeriodData, thisPeriodTime, true);

                // end period
                _strategy.EndPeriod();

                // update last period time
                lastPeriodTime = thisPeriodTime;
            }

            // finish evaluation
            _strategy.Finish();

            // Sell all equities forcibly.
            ClearEquityForcibly(lastPeriodTime);

            // clear all pending instructions.
            _pendingInstructions.Clear();

            // update 'evaluatable' flag to avoid this function be called twice
            _evaluatable = false;
        }

        private void ClearEquityForcibly(DateTime lastPeriodTime)
        {
            var codes = _equityManager.GetAllEquityCodes();
            foreach (var code in codes)
            {
                var equities = _equityManager.GetEquityDetails(code);
                int totalVolume = equities.Sum(e => e.Volume);

                if (totalVolume <= 0)
                {
                    throw new InvalidOperationException("total volume should be greater than zero, logic error");
                }

                Bar bar;
                if (!_provider.GetLastEffectiveData(code, lastPeriodTime, out bar))
                {
                    throw new InvalidOperationException(
                        string.Format("failed to get last data for code {0}, logic error", code));
                }

                Transaction transaction = new Transaction()
                {
                    Action = TradingAction.CloseLong,
                    Commission = 0.0,
                    ExecutionTime = lastPeriodTime,
                    InstructionId = long.MaxValue,
                    Object = _allTradingObjects[_tradingObjectIndexByCode[code]],
                    Price = bar.ClosePrice,
                    Succeeded = false,
                    SubmissionTime = lastPeriodTime,
                    Volume = totalVolume
                };

                UpdateTransactionCommission(transaction);

                if (!ExecuteTransaction(transaction, false))
                {
                    throw new InvalidOperationException(
                        string.Format("failed to execute transaction, logic error", code));
                }
            }
        }

        private void RunPendingInstructions(
            Bar[] tradingData, 
            DateTime time, 
            bool forCurrentPeriod)
        {
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

                if (forCurrentPeriod)
                {
                    if (!option.HasFlag(TradingPriceOption.CurrentPeriod))
                    {
                        continue;
                    }
                }
                else
                {
                    if (option.HasFlag(TradingPriceOption.CurrentPeriod))
                    {
                        throw new InvalidProgramException("Logic error, all transaction expect to be executed in today should have been fully executed");
                    }
                }

                int tradingObjectIndex = _tradingObjectIndexByCode[instruction.Object.Code];
                if (tradingData[tradingObjectIndex].Invalid())
                {
                    if (forCurrentPeriod)
                    {
                        throw new InvalidOperationException(
                            string.Format("the data for trading object {0} is invalid", instruction.Object.Code));
                    }
                    else
                    {
                        continue;
                    }
                }

                Transaction transaction = BuildTransactionFromInstruction(
                    instruction,
                    time,
                    tradingData[tradingObjectIndex]);

                // execute transaction
                ExecuteTransaction(transaction, true);

                // remove instruction that has been executed
                _pendingInstructions[i] = null;
            }

            // compact pending instruction list
            _pendingInstructions = _pendingInstructions.Where(i => i != null).ToList();
        }

        private bool ExecuteTransaction(Transaction transaction, bool notifyTransactionStatus)
        {
            string error;
            bool succeeded = _equityManager.ExecuteTransaction(transaction, out error);

            transaction.Succeeded = succeeded;
            transaction.Error = error;

            if (notifyTransactionStatus)
            {
                // notify transaction status
                _strategy.NotifyTransactionStatus(transaction);
            }

            // add to history
            _transactionHistory.Add(transaction);

            return succeeded;
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
                InstructionId = instruction.ID,
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
