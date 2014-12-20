using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy;

namespace TradingStrategyEvaluation
{
    /// <summary>
    /// Predicate the action according to latest data and strategy
    /// </summary>
    public sealed class TradingStrategyPredicator
    {
        private readonly ITradingStrategy _strategy;
        private readonly IDictionary<ParameterAttribute, object> _strategyParameterValues;
        private readonly ITradingDataProvider _provider;
        private readonly EquityManager _equityManager;
        private readonly StandardEvaluationContext _context;
        private ITradingObject[] _allTradingObjects;
        private DateTime[] _firstNonWarmupDataPeriods;
        private List<Position> _unprocessedActivePositions;

        private List<Instruction> _pendingInstructions = new List<Instruction>();

        private bool _predicatable = true;

        private List<Transaction> _predicatedTransactions = new List<Transaction>();

        public IEnumerable<Position> ActivePositions
        {
            get { return _equityManager.GetAllPositionCodes()
                .SelectMany(_equityManager.GetPositionDetails); }
        }

        public IEnumerable<Transaction> PredicatedTransactions
        {
            get { return _predicatedTransactions; }
        }

        public TradingStrategyPredicator(
            double initialCapital,
            double currentCapital,
            ITradingStrategy strategy, 
            IDictionary<ParameterAttribute, object> strategyParameters, 
            ITradingDataProvider provider, 
            StockBlockRelationshipManager relationshipManager,
            IEnumerable<Position> activePositions,
            ILogger logger)
        {
            if (strategy == null || provider == null)
            {
                throw new ArgumentNullException();
            }

            _strategy = strategy;
            _strategyParameterValues = strategyParameters;

            _provider = provider;
           
            _equityManager = new EquityManager(new SimpleCapitalManager(initialCapital, currentCapital));
            _unprocessedActivePositions = activePositions.ToList();

            _context = new StandardEvaluationContext(_provider, _equityManager, logger, relationshipManager);
        }

        public void Predicate()
        {
            if (!_predicatable)
            {
                throw new InvalidOperationException("Predicate() can be called only once");
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

                // add active positions
                if (_unprocessedActivePositions.Any())
                {
                    var positions = _unprocessedActivePositions.Where(p => p.BuyTime <= thisPeriodTime);
                    if (positions.Any())
                    {
                        foreach (var position in positions)
                        {
                            _equityManager.ManualAddPosition(position);
                        }

                        _unprocessedActivePositions = _unprocessedActivePositions.Where(p => p.BuyTime > thisPeriodTime).ToList();
                    }
                }

                // evaluate bar data
                _strategy.Evaluate(_allTradingObjects, thisPeriodData);

                // get instructions and add them to pending instruction list
                var instructions = _strategy.RetrieveInstructions().ToArray();

                if (instructions.Any())
                {
                    if (thisPeriodTime == periods[periods.Length - 1])
                    {
                        _predicatedTransactions = instructions
                            .Select(ins => BuildTransactionFromInstruction(ins, thisPeriodTime, thisPeriodData[ins.TradingObject.Index]))
                            .ToList();
                    }
                    else
                    {
                        _pendingInstructions.AddRange(instructions);

                        // run instructions for current period
                        RunPendingInstructions(lastPeriodData, thisPeriodData, thisPeriodTime);
                    }
                }

                // end period
                _strategy.EndPeriod();

                // reset current period data in context
                _context.SetCurrentPeriodData(null);

                // update last period time and data
                lastPeriodTime = thisPeriodTime;
                lastPeriodData = thisPeriodData;
            }

            // finish evaluation
            _strategy.Finish();

            // clear all pending instructions.
            _pendingInstructions.Clear();

            // update flag to avoid this function be called twice
            _predicatable = false;
        }


        private void RunPendingInstructions(
            Bar[] lastTradingData,
            Bar[] currentTradingData, 
            DateTime time)
        {
            var pendingTransactions = new Transaction[_pendingInstructions.Count];

            for (var i = 0; i < _pendingInstructions.Count; ++i)
            {
                var instruction = _pendingInstructions[i];

                var tradingObjectIndex = instruction.TradingObject.Index;
                var currentTradingDataOfObject = currentTradingData[tradingObjectIndex];
                if (currentTradingDataOfObject.Time == Bar.InvalidTime)
                {
                    // remove the instruction and continue;
                    _pendingInstructions[i] = null;
                    continue;
                }

                var transaction = BuildTransactionFromInstruction(
                    instruction,
                    time,
                    currentTradingDataOfObject);

                pendingTransactions[i] =transaction;
            }


            // always execute transaction according to the original order, so the strategy itself
            // can decide the order.
            for (int i = 0; i < pendingTransactions.Length; ++i)
            {
                if (pendingTransactions[i] != null)
                {
                    ExecuteTransaction(pendingTransactions[i]);

                    _pendingInstructions[i] = null;
                }
            }

            // compact pending instruction list
            _pendingInstructions = _pendingInstructions.Where(i => i != null).ToList();
        }

        private void ExecuteTransaction(Transaction transaction)
        {
            transaction.Succeeded = false;
            transaction.Error = "No execution";

            // notify transaction status
            _strategy.NotifyTransactionStatus(transaction);
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
                Name = instruction.TradingObject.Name,
                Price = bar.ClosePrice,
                Succeeded = false,
                SubmissionTime = instruction.SubmissionTime,
                Volume = instruction.Volume,
                SellingType = instruction.SellingType,
                StopLossGapForBuying = instruction.StopLossGapForBuying,
                StopLossPriceForSelling = instruction.StopLossPriceForSelling,
                PositionIdForSell = instruction.PositionIdForSell,
                Comments = instruction.Comments,
                RelatedObjects = instruction.RelatedObjects
            };

            return transaction;
        }
    }
}
