using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy;

namespace TradingStrategyEvaluation
{
    public sealed class TradingStrategyEvaluator
    {
        private readonly ITradingStrategy _strategy;
        private readonly IDictionary<ParameterAttribute, object> _strategyParameterValues;
        private readonly ITradingDataProvider _provider;
        private readonly EquityManager _equityManager;
        private readonly StandardEvaluationContext _context;
        private readonly TradingSettings _settings;
        private readonly TradingTracker _tradingTracker;
        private ITradingObject[] _allTradingObjects;
        private List<Instruction> _pendingInstructions = new List<Instruction>();

        private bool _evaluatable = true;

        public TradingTracker Tracker
        {
            get { return _tradingTracker; }
        }

        public IEnumerable<Position> ClosedPositions
        {
            get { return _equityManager.ClosedPositions; }
        }

        public EventHandler<EvaluationProgressEventArgs> OnEvaluationProgress;

        public TradingStrategyEvaluator(
            double initalCapital, 
            ITradingStrategy strategy, 
            IDictionary<ParameterAttribute, object> strategyParameters, 
            ITradingDataProvider provider, 
            TradingSettings settings,
            ILogger logger)
        {
            if (strategy == null || provider == null || settings == null)
            {
                throw new ArgumentNullException();
            }

            _strategy = strategy;
            _strategyParameterValues = strategyParameters;

            _provider = provider;
           
            _settings = settings;

            _equityManager = new EquityManager(initalCapital);
            _context = new StandardEvaluationContext(_provider, _equityManager, logger);
            _tradingTracker = new TradingTracker(initalCapital);
        }

        public void Evaluate()
        {
            if (!_evaluatable)
            {
                throw new InvalidOperationException("Evalute() can be called only once");
            }

            // initialize context
            _strategy.Initialize(_context, _strategyParameterValues);

            // Get all trading objects
            _allTradingObjects = _provider.GetAllTradingObjects();
            Action<ITradingObject> warmupAction = 
                obj =>
                {
                    var warmupData = _provider.GetWarmUpData(obj.Index);
                    if (warmupData != null)
                    {
                        foreach (var bar in warmupData)
                        {
                            _strategy.WarmUp(obj, bar);
                        }
                    }
                };

            // warm up
            foreach (var tradingObject in _allTradingObjects)
            {
                warmupAction(tradingObject);
            }

            // evaluating
            var lastPeriodTime = DateTime.MinValue;
            Bar[] lastPeriodData = null;
            var periods = _provider.GetAllPeriodsOrdered();
            for (var periodIndex = 0; periodIndex < periods.Length; ++periodIndex)
            {
                var thisPeriodTime = periods[periodIndex];
                var thisPeriodData = _provider.GetDataOfPeriod(thisPeriodTime);

                if (thisPeriodData.Length != _allTradingObjects.Length)
                {
                    throw new InvalidOperationException("the number of data returned does not match the number of trading object");
                }
                
                // set current period data in context
                _context.SetCurrentPeriodData(thisPeriodData);

                // start a new period
                _strategy.StartPeriod(thisPeriodTime);
                
                // run pending instructions left over from previous period
                RunPendingInstructions(lastPeriodData, thisPeriodData, thisPeriodTime, false);

                // check data
                if (thisPeriodData.Any(bar => bar.Time != Bar.InvalidTime && bar.Time != thisPeriodTime))
                {
                    throw new InvalidOperationException("Time in bar data is different with the time returned by data provider");
                }

                // evaluate bar data
                _strategy.Evaluate(_allTradingObjects, thisPeriodData);


                // get instructions and add them to pending instruction list
                var instructions = _strategy.RetrieveInstructions().ToArray();

                if (instructions.Any())
                {
                    _pendingInstructions.AddRange(instructions);

                    // run instructions for current period
                    RunPendingInstructions(lastPeriodData, thisPeriodData, thisPeriodTime, true);
                }

                // end period
                _strategy.EndPeriod();

                // reset current period data in context
                _context.SetCurrentPeriodData(null);

                // update last period time and data
                lastPeriodTime = thisPeriodTime;
                lastPeriodData = thisPeriodData;

                // update progress event
                if (OnEvaluationProgress != null)
                {
                    OnEvaluationProgress(
                        this, 
                        new EvaluationProgressEventArgs(
                            thisPeriodTime, 
                            (double)(periodIndex + 1) / periods.Length));
                }
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
            var codes = _equityManager.GetAllPositionCodes();
            foreach (var code in codes)
            {
                var equities = _equityManager.GetPositionDetails(code);
                var totalVolume = equities.Sum(e => e.Volume);

                if (totalVolume <= 0)
                {
                    throw new InvalidOperationException("total volume should be greater than zero, logic error");
                }

                Bar bar;
                var index = _provider.GetIndexOfTradingObject(code);

                if (!_provider.GetLastEffectiveBar(index, lastPeriodTime, out bar))
                {
                    throw new InvalidOperationException(
                        string.Format("failed to get last data for code {0}, logic error", code));
                }

                var transaction = new Transaction
                {
                    Action = TradingAction.CloseLong,
                    Commission = 0.0,
                    ExecutionTime = lastPeriodTime,
                    InstructionId = long.MaxValue,
                    Code = code,
                    Price = bar.ClosePrice,
                    SellingType = SellingType.ByVolume,
                    Succeeded = false,
                    SubmissionTime = lastPeriodTime,
                    Volume = totalVolume,
                    Comments = "clear forcibly"
                };

                UpdateTransactionCommission(transaction, _allTradingObjects[index]);

                if (!ExecuteTransaction(transaction, false))
                {
                    throw new InvalidOperationException(
                        string.Format("failed to execute transaction, logic error", code));
                }
            }
        }

        private void RunPendingInstructions(
            Bar[] lastTradingData,
            Bar[] currentTradingData, 
            DateTime time, 
            bool forCurrentPeriod)
        {
            var pendingTansactions = new Dictionary<int, Transaction>();

            for (var i = 0; i < _pendingInstructions.Count; ++i)
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

                var tradingObjectIndex = instruction.TradingObject.Index;
                var currentTradingDataOfObject = currentTradingData[tradingObjectIndex];
                if (currentTradingDataOfObject.Time == Bar.InvalidTime)
                {
                    if (forCurrentPeriod)
                    {
                        throw new InvalidOperationException(
                            string.Format("the data for trading object {0} is invalid", instruction.TradingObject.Code));
                    }

                    // remove the instruction and continue;
                    _pendingInstructions[i] = null;
                    continue;
                }

                // exclude 涨停/跌停
                if (!forCurrentPeriod)
                {
                    const double MaxChangesCoefficient = 0.95;

                    var lastTradingDataOfObject = lastTradingData[tradingObjectIndex];
                    if (lastTradingDataOfObject.Time != Bar.InvalidTime)
                    {
                        if (option.HasFlag(TradingPriceOption.OpenPrice))
                        {
                            double priceChangeRatio = Math.Abs(currentTradingDataOfObject.OpenPrice - lastTradingDataOfObject.ClosePrice) / lastTradingDataOfObject.ClosePrice;
                            
                            if (instruction.Action == TradingAction.OpenLong
                                && priceChangeRatio >= instruction.TradingObject.LimitUpRatio * MaxChangesCoefficient)
                            {
                                _context.Log(
                                    string.Format(
                                        "{0} price {1:0.0000} hit limit up in {2:yyyy-MM-dd}, failed to execute transaction",
                                        instruction.TradingObject.Code,
                                        currentTradingDataOfObject.OpenPrice,
                                        time));

                                // remove the instruction and continue;
                                _pendingInstructions[i] = null;
                                continue;
                            }

                            if (instruction.Action == TradingAction.CloseLong
                                && priceChangeRatio >= instruction.TradingObject.LimitDownRatio * MaxChangesCoefficient)
                            {
                                _context.Log(
                                    string.Format(
                                        "{0} price {1:0.0000} hit limit down in {2:yyyy-MM-dd}, failed to execute transaction",
                                        instruction.TradingObject.Code,
                                        currentTradingDataOfObject.OpenPrice,
                                        time));

                                // remove the instruction and continue;
                                _pendingInstructions[i] = null;
                                continue;
                            }
                        }
                    }
                }

                // exclude 一字板
                if (currentTradingDataOfObject.HighestPrice == currentTradingDataOfObject.LowestPrice)
                {
                    _context.Log(
                        string.Format(
                            "{0} price is locked down in {1:yyyy-MM-dd}, failed to execute transaction",
                            instruction.TradingObject.Code,
                            time));

                    // remove the instruction and continue;
                    _pendingInstructions[i] = null;
                    continue;
                }

                var transaction = BuildTransactionFromInstruction(
                    instruction,
                    time,
                    currentTradingDataOfObject);

                pendingTansactions.Add(i, transaction);
            }

            var orderedTransactions = pendingTansactions.Values
                .OrderBy(t => t, new Transaction.DefaultComparer());

            // execute the close long transaction firstly
            foreach (var transaction in orderedTransactions)
            {
                // execute transaction
                ExecuteTransaction(transaction, true);
            }

            foreach (var index in pendingTansactions.Keys)
            {
                // remove instruction that has been executed
                _pendingInstructions[index] = null;
            }

            // compact pending instruction list
            _pendingInstructions = _pendingInstructions.Where(i => i != null).ToList();
        }

        private bool ExecuteTransaction(Transaction transaction, bool notifyTransactionStatus)
        {
            string error;

            CompletedTransaction completedTransaction;
            var succeeded = _equityManager.ExecuteTransaction(
                transaction,
                true,
                out completedTransaction,
                out error);

            transaction.Succeeded = succeeded;
            transaction.Error = error;

            if (notifyTransactionStatus)
            {
                // notify transaction status
                _strategy.NotifyTransactionStatus(transaction);
            }

            // add to history
            _tradingTracker.AddTransaction(transaction);
            if (completedTransaction != null)
            {
                _tradingTracker.AddCompletedTransaction(completedTransaction);
            }

            // log transaction
            _context.Log(transaction.Print());

            return succeeded;
        }

        private Transaction BuildTransactionFromInstruction(Instruction instruction, DateTime time, Bar bar)
        {
            if (time != bar.Time)
            {
                throw new ArgumentException("Inconsistent time in bar and time parameters");
            }

            if ((instruction.Action == TradingAction.OpenLong 
                    && instruction.Volume % instruction.TradingObject.VolumePerBuyingUnit != 0)
                || (instruction.Action == TradingAction.CloseLong
                    && instruction.Volume % instruction.TradingObject.VolumePerSellingUnit != 0))
            {
                throw new InvalidOperationException("The volume of transaction does not meet trading object's requirement");
            }

            var transaction = new Transaction
            {
                Action = instruction.Action,
                Commission = 0.0,
                ExecutionTime = time,
                InstructionId = instruction.Id,
                Code = instruction.TradingObject.Code,
                Price = CalculateTransactionPrice(bar, instruction),
                Succeeded = false,
                SubmissionTime = instruction.SubmissionTime,
                Volume = instruction.Volume,
                SellingType = instruction.SellingType,
                StopLossPriceForSell = instruction.StopLossPriceForSell,
                PositionIdForSell = instruction.PositionIdForSell,
                Comments = instruction.Comments,
            };

            // update commission
            UpdateTransactionCommission(transaction, instruction.TradingObject);

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
                price += _settings.Spread * instruction.TradingObject.MinPriceUnit;
            }
            else if (instruction.Action == TradingAction.CloseLong)
            {
                price -= _settings.Spread * instruction.TradingObject.MinPriceUnit;

                if (price < instruction.TradingObject.MinPriceUnit)
                {
                    price = instruction.TradingObject.MinPriceUnit;
                }
            }

            return price;
        }

        private void UpdateTransactionCommission(Transaction transaction, ITradingObject tradingObject)
        {
            if (tradingObject.Code != transaction.Code)
            {
                throw new ArgumentException("Code in transaction and trading object are different");
            }

            CommissionSettings commission;

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

            if (commission.Type == CommissionSettings.CommissionType.ByAmount)
            {
                transaction.Commission = transaction.Price * transaction.Volume * commission.Tariff;
            }
            else if (commission.Type == CommissionSettings.CommissionType.ByVolume)
            {
                var hands = Math.Ceiling((double)transaction.Volume / tradingObject.VolumePerHand);

                transaction.Commission = commission.Tariff * hands;
            }

// ReSharper disable PossibleLossOfFraction
            transaction.Commission = ((long)(transaction.Commission * 10000.0)) / 10000L;
// ReSharper restore PossibleLossOfFraction
        }
    }
}
