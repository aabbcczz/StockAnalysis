using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using StockAnalysis.Share;
using TradingStrategy;

namespace TradingStrategyEvaluation
{
    public sealed class TradingStrategyEvaluator
    {
        private readonly int _numberOfAccounts;
        private readonly int _accountId;
        private readonly ITradingStrategy _strategy;
        private readonly IDictionary<ParameterAttribute, object> _strategyParameterValues;
        private readonly ITradingDataProvider _provider;
        private readonly EquityManager _equityManager;
        private readonly StandardEvaluationContext _context;
        private readonly TradingSettings _settings;
        private readonly TradingTracker _tradingTracker;
        private ITradingObject[] _allTradingObjects;
        private DateTime[] _firstNonWarmupDataPeriods;

        private List<Instruction> _currentPeriodInstructions = new List<Instruction>();
        private List<Instruction> _nextPeriodInstructions = new List<Instruction>();

        private bool _evaluatable = true;
        private int _activeAccountId = -1;

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
            int numberOfAccounts,
            int accountId,
            ICapitalManager capitalManager,
            ITradingStrategy strategy, 
            IDictionary<ParameterAttribute, object> strategyParameters, 
            ITradingDataProvider provider, 
            StockBlockRelationshipManager relationshipManager,
            TradingSettings settings,
            ILogger logger,
            StreamWriter dumpDataWriter)
        {
            if (numberOfAccounts <= 0 || accountId < 0 || accountId >= numberOfAccounts)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (strategy == null || provider == null || settings == null)
            {
                throw new ArgumentNullException();
            }

            _numberOfAccounts = numberOfAccounts;
            _accountId = accountId;
            _strategy = strategy;
            _strategyParameterValues = strategyParameters;

            _provider = provider;
           
            _settings = settings;

            _equityManager = new EquityManager(capitalManager, _settings.PositionFrozenDays);
            _context = new StandardEvaluationContext(_provider, _equityManager, logger, settings, dumpDataWriter, relationshipManager);
            _tradingTracker = new TradingTracker(capitalManager.InitialCapital);
        }

        public void Evaluate()
        {
            if (!_evaluatable)
            {
                throw new InvalidOperationException("Evaluate() can be called only once");
            }

            // initialize context
            _strategy.Initialize(_context, _strategyParameterValues);

            // Get all trading objects
            _allTradingObjects = _provider.GetAllTradingObjects();

            // Get last warm up data periods
            _firstNonWarmupDataPeriods = _provider.GetFirstNonWarmupDataPeriods();

            // evaluating
            var lastPeriodTime = DateTime.MinValue;
            Bar[] lastPeriodData = null;
            var periods = _provider.GetAllPeriodsOrdered();

            bool fullTradingDataReached = false;

            for (var periodIndex = 0; periodIndex < periods.Length; ++periodIndex)
            {
                var thisPeriodTime = periods[periodIndex];
                var thisPeriodData = _provider.GetDataOfPeriod(thisPeriodTime);

                if (thisPeriodData.Length != _allTradingObjects.Length)
                {
                    throw new InvalidOperationException("the number of data returned does not match the number of trading object");
                }
                
                // process pure warm up data.
                if (_firstNonWarmupDataPeriods.All(t => t > thisPeriodTime))
                {
                    // update metrics that registered by strategy
                    _context.MetricManager.BeginUpdateMetrics();

                    _context.MetricManager.UpdateMetrics(_allTradingObjects, thisPeriodData);
                    
                    _context.MetricManager.EndUpdateMetrics();

                    continue;
                }

                // remove warm up data if necessary
                var originalThisPeriodData = thisPeriodData;
                if (!fullTradingDataReached)
                {
                    if (_firstNonWarmupDataPeriods.All(t => t <= thisPeriodTime))
                    {
                        // from now on, all data are useful trading data
                        fullTradingDataReached = true;
                    }
                    else
                    {
                        thisPeriodData = new Bar[originalThisPeriodData.Length];
                        Array.Copy(originalThisPeriodData, thisPeriodData, thisPeriodData.Length);

                        for (int i =0; i < thisPeriodData.Length; ++i)
                        {
                            if (thisPeriodData[i].Time < _firstNonWarmupDataPeriods[i])
                            {
                                thisPeriodData[i].Time = Bar.InvalidTime;
                            }
                        }
                    }
                }

                // set current period data in context
                _context.SetCurrentPeriodData(thisPeriodData);

                // start a new period
                _strategy.StartPeriod(thisPeriodTime);

                // assign all instructions in next period to current period because new period starts
                _currentPeriodInstructions.Clear();
                _currentPeriodInstructions.AddRange(_nextPeriodInstructions);
                _nextPeriodInstructions.Clear();

                // run instructions left over from previous period
                RunCurrentPeriodInstructions(lastPeriodData, thisPeriodData, thisPeriodTime);

#if DEBUG
                // check data
                if (thisPeriodData.Any(bar => bar.Time != Bar.InvalidTime && bar.Time != thisPeriodTime))
                {
                    throw new InvalidOperationException("Time in bar data is different with the time returned by data provider");
                }
#endif

                // update metrics that registered by strategy. 
                // here we should always use original data to update metrics.
                _context.MetricManager.BeginUpdateMetrics();
                _context.MetricManager.UpdateMetrics(_allTradingObjects, originalThisPeriodData);
                _context.MetricManager.EndUpdateMetrics();

                // update position lasted period count
                foreach (var code in _context.GetAllPositionCodes())
                {
                    int tradingObjectIndex = _provider.GetIndexOfTradingObject(code);

                    if (thisPeriodData[tradingObjectIndex].Time != Bar.InvalidTime)
                    {
                        foreach (var position in _context.GetPositionDetails(code))
                        {
                            if (position.BuyTime < thisPeriodTime)
                            {
                                position.IncreaseLastedPeriodCount();
                            }
                        }
                    }
                }

                // evaluate bar data
                _strategy.Evaluate(_allTradingObjects, thisPeriodData);

                // get instructions and add them to pending instruction list
                var instructions = _strategy.RetrieveInstructions().ToArray();

                if (instructions.Any(i => i.Action == TradingAction.OpenLong))
                {
                    // update active account only if there is open long instruction
                    _activeAccountId++;
                    _activeAccountId %= _numberOfAccounts;

                    // decide if instructions should be excuted in current account
                    if (_activeAccountId != _accountId)
                    {
                        // remove OpenLong instructions
                        instructions = instructions.Where(i => i.Action != TradingAction.OpenLong).ToArray();
                    }
                }

                if (instructions.Any())
                {
                    foreach (var instruction in instructions)
                    {
                        UpdateInstructionWithDefaultPriceWhenNecessary(instruction);

                        if (instruction.Price.Period == TradingPricePeriod.CurrentPeriod)
                        {
                            _currentPeriodInstructions.Add(instruction);
                        }
                        else if (instruction.Price.Period == TradingPricePeriod.NextPeriod)
                        {
                            _nextPeriodInstructions.Add(instruction);
                        }
                        else
                        {
                            throw new InvalidProgramException("unsupported price period");
                        }
                    }

                    // run instructions for current period
                    RunCurrentPeriodInstructions(lastPeriodData, thisPeriodData, thisPeriodTime);
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
            _currentPeriodInstructions.Clear();
            _nextPeriodInstructions.Clear();

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
                    Name = _allTradingObjects[index].Name,
                    Price = bar.ClosePrice,
                    SellingType = SellingType.ByVolume,
                    Succeeded = false,
                    SubmissionTime = lastPeriodTime,
                    Volume = totalVolume,
                    Comments = "clear forcibly"
                };

                UpdateTransactionCommission(transaction, _allTradingObjects[index]);

                if (!ExecuteTransaction(transaction, false, true))
                {
                    throw new InvalidOperationException(
                        string.Format("failed to execute transaction, logic error", code));
                }
            }
        }

        private void RunCurrentPeriodInstructions(
            Bar[] lastPeriodTradingData,
            Bar[] currentPeriodTradingData, 
            DateTime time)
        {
            var readyInstructions = new List<Tuple<Instruction, double>>(_currentPeriodInstructions.Count);

            foreach (var instruction in _currentPeriodInstructions)
            {
                var tradingObjectIndex = instruction.TradingObject.Index;
                var currentBarOfObject = currentPeriodTradingData[tradingObjectIndex];

                if (currentBarOfObject.Time == Bar.InvalidTime)
                {
                    _context.Log(string.Format("the data for trading object {0} is invalid, can't execute instruction", instruction.TradingObject.Code));
                    continue;
                }

                // exclude 开盘涨停/跌停
                if (lastPeriodTradingData == null)
                {
                    continue;
                }

                var lastBarOfObject = lastPeriodTradingData[tradingObjectIndex];
                const double MaxChangesCoefficient = 0.99;

                if (lastBarOfObject.Time != Bar.InvalidTime)
                {
                    if (instruction.Price.Option == TradingPriceOption.OpenPrice
                        || instruction.Price.Option == TradingPriceOption.CustomPrice)
                    {
                        double priceChangeRatio = Math.Abs(currentBarOfObject.OpenPrice - lastBarOfObject.ClosePrice) / lastBarOfObject.ClosePrice;

                        if (instruction.Action == TradingAction.OpenLong
                            && priceChangeRatio >= instruction.TradingObject.LimitUpRatio * MaxChangesCoefficient)
                        {
                            _context.Log(
                                string.Format(
                                    "{0} price {1:0.0000} hit limit up in {2:yyyy-MM-dd}, failed to execute transaction",
                                    instruction.TradingObject.Code,
                                    currentBarOfObject.OpenPrice,
                                    time));

                            continue;
                        }

                        if (instruction.Action == TradingAction.CloseLong
                            && priceChangeRatio >= instruction.TradingObject.LimitDownRatio * MaxChangesCoefficient)
                        {
                            _context.Log(
                                string.Format(
                                    "{0} price {1:0.0000} hit limit down in {2:yyyy-MM-dd}, failed to execute transaction",
                                    instruction.TradingObject.Code,
                                    currentBarOfObject.OpenPrice,
                                    time));

                            continue;
                        }
                    }
                }

                // exclude 一字板
                if (currentBarOfObject.HighestPrice == currentBarOfObject.LowestPrice)
                {
                    if ((currentBarOfObject.LowestPrice < lastBarOfObject.ClosePrice
                        && instruction.Action == TradingAction.CloseLong)
                        || (currentBarOfObject.LowestPrice > lastBarOfObject.ClosePrice
                            && instruction.Action == TradingAction.OpenLong))
                    _context.Log(
                        string.Format(
                            "{0} price is locked down in {1:yyyy-MM-dd}, failed to execute transaction",
                            instruction.TradingObject.Code,
                            time));

                    continue;
                }

                double price = CalculateTransactionPrice(
                    currentBarOfObject, 
                    lastBarOfObject,
                    instruction);

                // adjust buy price
                if (instruction.Action == TradingAction.OpenLong)
                {
                    price *= _context.GlobalSettings.TrueBuyPriceScale;
                }

                // Exclude unrealistic price.
                if ((_settings.IsLowestPriceAchievable && price < currentBarOfObject.LowestPrice)
                    || (!_settings.IsLowestPriceAchievable && price <= currentBarOfObject.LowestPrice)
                    || price > currentBarOfObject.HighestPrice)
                {
                    _context.Log(
                        string.Format(
                            "{0} price {1:0.000} in {2:yyyy-MM-dd} is not achievable",
                            instruction.TradingObject.Code,
                            price,
                            time));

                    continue;
                }

                readyInstructions.Add(Tuple.Create(instruction, price));
            }
            
            int totalNumberOfObjectsToBeEstimated = readyInstructions.Count(t => t.Item1.Action == TradingAction.OpenLong);
            int maxNewPositionCount = _strategy.GetMaxNewPositionCount(totalNumberOfObjectsToBeEstimated);

            var pendingTransactions = new List<Transaction>(readyInstructions.Count);
            foreach (var readyInstruction in readyInstructions)
            {
                var instruction = readyInstruction.Item1;
                double price = readyInstruction.Item2;

                if (instruction.Action == TradingAction.OpenLong)
                {
                    if (maxNewPositionCount <= 0)
                    {
                        _context.Log(string.Format(
                            "Max new position count reached, ignore this instruction: {0}/{1}. //{2}",
                            instruction.TradingObject.Code,
                            instruction.TradingObject.Name,
                            instruction.Comments));

                        continue;
                    }

                    _strategy.EstimateStoplossAndSizeForNewPosition(instruction, price, totalNumberOfObjectsToBeEstimated);
                    if (instruction.Volume == 0)
                    {
                        _context.Log(string.Format(
                            "The volume of instruction for {0}/{1} is 0. //{2}",
                            instruction.TradingObject.Code,
                            instruction.TradingObject.Name,
                            instruction.Comments));

                        continue;
                    }

                    --maxNewPositionCount;
                }

                var transaction = BuildTransactionFromInstruction(
                    instruction,
                    time,
                    price);

                pendingTransactions.Add(transaction);
            }

            // always execute transaction according to the original order, so the strategy itself
            // can decide the order.
            foreach (var transaction in pendingTransactions)
            {
                ExecuteTransaction(transaction, true);
            }

            // compact pending instruction list
            _currentPeriodInstructions.Clear();
        }

        private bool ExecuteTransaction(Transaction transaction, bool notifyTransactionStatus, bool forcibly = false)
        {
            string error;

            CompletedTransaction completedTransaction;
            var succeeded = _equityManager.ExecuteTransaction(
                            transaction,
                            _settings.AllowNegativeCapital,
                            out completedTransaction,
                            out error,
                            forcibly);

            if (!succeeded)
            {
                if (transaction.Action == TradingAction.OpenLong)
                {
                    // HACK: try to adjust volume to reduce money used and make transaction succeeded.
                    var volume = (int)((double)transaction.Volume / 1.1);
                    volume -= volume % 100;

                    if (volume > 0)
                    {
                        transaction.Volume = volume;
                        succeeded = _equityManager.ExecuteTransaction(
                                        transaction,
                                        _settings.AllowNegativeCapital,
                                        out completedTransaction,
                                        out error);
                    }
                }
            }

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

        private Transaction BuildTransactionFromInstruction(Instruction instruction, DateTime time, double price)
        {
            if ((instruction.Action == TradingAction.OpenLong 
                    && instruction.Volume % instruction.TradingObject.VolumePerBuyingUnit != 0)
                || (instruction.Action == TradingAction.CloseLong
                    && instruction.Volume % instruction.TradingObject.VolumePerSellingUnit != 0))
            {
                throw new InvalidOperationException("The volume of transaction does not meet trading object's requirement");
            }

            var transaction = new Transaction(instruction, price)
            {
                Commission = 0.0,
                ExecutionTime = time,
                Succeeded = false,
            };

            // update commission
            UpdateTransactionCommission(transaction, instruction.TradingObject);

            return transaction;
        }

        private bool IsInstructionForStopLossing(Instruction instruction)
        {
            return instruction.Action == TradingAction.CloseLong 
                && (instruction as CloseInstruction).SellingType == SellingType.ByStopLossPrice;
        }

        private void UpdateInstructionWithDefaultPriceWhenNecessary(Instruction instruction)
        {
            TradingPrice price;

            if (instruction.Price == null)
            {
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
        private double CalculateTransactionPrice(
            Bar currentPeriodBar, 
            Bar previousPeriodBar,
            Instruction instruction)
        {
            double realPrice;

            if (IsInstructionForStopLossing(instruction))
            {
                realPrice = (instruction as CloseInstruction).StopLossPriceForSelling;
            }
            else
            {
                realPrice = instruction.Price.GetRealPrice(currentPeriodBar, previousPeriodBar);
            }


            // count the spread now
            if (instruction.Action == TradingAction.OpenLong)
            {
                realPrice += _settings.Spread * instruction.TradingObject.MinPriceUnit;
            }
            else if (instruction.Action == TradingAction.CloseLong)
            {
                realPrice -= _settings.Spread * instruction.TradingObject.MinPriceUnit;

                if (realPrice < instruction.TradingObject.MinPriceUnit)
                {
                    realPrice = instruction.TradingObject.MinPriceUnit;
                }
            }

            return realPrice;
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
