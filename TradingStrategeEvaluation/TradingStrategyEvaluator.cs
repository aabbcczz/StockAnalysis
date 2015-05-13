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
        private DateTime[] _firstNonWarmupDataPeriods;

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
            ICapitalManager capitalManager,
            ITradingStrategy strategy, 
            IDictionary<ParameterAttribute, object> strategyParameters, 
            ITradingDataProvider provider, 
            StockBlockRelationshipManager relationshipManager,
            TradingSettings settings,
            ILogger logger,
            IDataDumper dumper)
        {
            if (strategy == null || provider == null || settings == null)
            {
                throw new ArgumentNullException();
            }

            _strategy = strategy;
            _strategyParameterValues = strategyParameters;

            _provider = provider;
           
            _settings = settings;

            _equityManager = new EquityManager(capitalManager);
            _context = new StandardEvaluationContext(_provider, _equityManager, logger, dumper, relationshipManager);
            _tradingTracker = new TradingTracker(capitalManager.InitialCapital);
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

                // run pending instructions left over from previous period
                RunPendingInstructions(lastPeriodData, thisPeriodData, thisPeriodTime, false);

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

        private void RunPendingInstructions(
            Bar[] lastTradingData,
            Bar[] currentTradingData, 
            DateTime time, 
            bool forCurrentPeriod)
        {
            var readyInstructions = new Tuple<Instruction, double>[_pendingInstructions.Count];

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

                // special processing for stop loss.
                if (!IsInstructionForStopLossing(instruction))
                {
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

                    if (instruction.Action == TradingAction.OpenLong)
                    {
                        // remove the instruction and continue;
                        _pendingInstructions[i] = null;
                    }

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

                double price = CalculateTransactionPrice(currentTradingDataOfObject, instruction);

                readyInstructions[i] = Tuple.Create(instruction, price);
            }

            int totalNumberOfObjectsToBeEstimated = readyInstructions.Count(t => t != null && t.Item1.Action == TradingAction.OpenLong);

            var pendingTransactions = new Transaction[_pendingInstructions.Count];
            for (int i = 0; i < readyInstructions.Length; ++i)
            {
                if (readyInstructions[i] != null)
                {
                    var instruction = readyInstructions[i].Item1;
                    double price = readyInstructions[i].Item2;

                    if (readyInstructions[i].Item1.Action == TradingAction.OpenLong)
                    {
                        _strategy.EstimateStoplossAndSizeForNewPosition(instruction, price, totalNumberOfObjectsToBeEstimated);
                        if (readyInstructions[i].Item1.Volume == 0)
                        {
                            _context.Log(string.Format("The volume of instruction for {0}/{1} is 0", instruction.TradingObject.Code, instruction.TradingObject.Name));

                            readyInstructions[i] = null;

                            // remove pending instruction because no transcation could be created for this, and we also won't keep this instruction.
                            _pendingInstructions[i] = null;

                            continue;
                        }
                    }

                    var transaction = BuildTransactionFromInstruction(
                        instruction,
                        time,
                        price);

                    pendingTransactions[i] = transaction;
                }
            }

            // always execute transaction according to the original order, so the strategy itself
            // can decide the order.
            for (int i = 0; i < pendingTransactions.Length; ++i)
            {
                if (pendingTransactions[i] != null)
                {
                    ExecuteTransaction(pendingTransactions[i], true);

                    _pendingInstructions[i] = null;
                }
            }

            // compact pending instruction list
            _pendingInstructions = _pendingInstructions.Where(instruction => instruction != null).ToList();
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

            var transaction = new Transaction
            {
                Action = instruction.Action,
                Commission = 0.0,
                ExecutionTime = time,
                InstructionId = instruction.Id,
                Code = instruction.TradingObject.Code,
                Name = instruction.TradingObject.Name,
                Price = price,
                Succeeded = false,
                SubmissionTime = instruction.SubmissionTime,
                Volume = instruction.Volume,
                SellingType = instruction.SellingType,
                StopLossGapForBuying = instruction.StopLossGapForBuying,
                StopLossPriceForSelling = instruction.StopLossPriceForSelling,
                PositionIdForSell = instruction.PositionIdForSell,
                Comments = instruction.Comments,
                RelatedObjects = instruction.RelatedObjects,
                ObservedMetricValues = instruction.ObservedMetricValues,
            };

            // update commission
            UpdateTransactionCommission(transaction, instruction.TradingObject);

            return transaction;
        }

        private bool IsInstructionForStopLossing(Instruction instruction)
        {
            return instruction.Action == TradingAction.CloseLong && instruction.SellingType == SellingType.ByStopLossPrice;
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

            if (IsInstructionForStopLossing(instruction))
            {
                price = instruction.StopLossPriceForSelling;
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
