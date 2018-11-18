namespace StockAnalysis.TradingStrategy.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Data;

    internal sealed class CloseInstructionPriceComparer : IComparer<CloseInstruction>
    {
        public int Compare(CloseInstruction x, CloseInstruction y)
        {
            // check period firstly
            if (x.Price.Period == TradingPricePeriod.CurrentPeriod && y.Price.Period == TradingPricePeriod.NextPeriod)
            {
                return -1;
            }
            else if (x.Price.Period == TradingPricePeriod.NextPeriod && y.Price.Period == TradingPricePeriod.CurrentPeriod)
            {
                return 0;
            }

            // same period now. check price option
            if (x.Price.Option == TradingPriceOption.OpenPrice)
            {
                if (y.Price.Option != TradingPriceOption.OpenPrice)
                {
                    return -1;
                }
                else
                {
                    return x.Id.CompareTo(y.Id);
                }
            }
            else if (x.Price.Option == TradingPriceOption.ClosePrice)
            {
                if (y.Price.Option != TradingPriceOption.ClosePrice)
                {
                    return 1;
                }
                else
                {
                    return x.Id.CompareTo(y.Id);
                }
            }
            else if (x.Price.Option == TradingPriceOption.CustomPrice)
            {
                if (y.Price.Option == TradingPriceOption.OpenPrice)
                {
                    return 1;
                }
                else if (y.Price.Option == TradingPriceOption.ClosePrice)
                {
                    return -1;
                }
                else if (y.Price.Option == TradingPriceOption.CustomPrice)
                {
                    if (x.Price.CustomPrice == y.Price.CustomPrice)
                    {
                        return x.Id.CompareTo(y.Id);
                    }
                    else
                    {
                        // the higher price, the first exit.
                        return y.Price.CustomPrice.CompareTo(x.Price.CustomPrice);
                    }
                }
                else
                {
                    throw new InvalidProgramException();
                }
            }
            else
            {
                throw new InvalidProgramException();
            }
        }
    }

    public sealed class CombinedStrategy : ITradingStrategy
    {
        private static bool _forceLoaded;

        private readonly bool _allowRemovingInstructionRandomly;
        private readonly ITradingStrategyComponent[] _components;
        private readonly IPositionSizingComponent _positionSizing;
        private readonly List<IMarketEnteringComponent> _marketEntering = new List<IMarketEnteringComponent>();
        private readonly List<IMarketExitingComponent> _marketExiting = new List<IMarketExitingComponent>();
        private readonly List<IBuyPriceFilteringComponent> _buyPriceFilters = new List<IBuyPriceFilteringComponent>();
        private readonly IStopLossComponent _stopLoss;
        private readonly IPositionAdjustingComponent _positionAdjusting;
        private readonly GlobalSettingsComponent _globalSettings;
        private readonly Position[] _emptyPositionArray = new Position[0];

        private readonly string _name;
        private readonly string _description;

        private IEvaluationContext _context;
        private List<Instruction> _instructionsInCurrentPeriod;
        private readonly Dictionary<long, Instruction> _activeInstructions = new Dictionary<long, Instruction>();
        private DateTime _period;
        private Random _random;

        public string Name
        {
            get { return _name; }
        }

        public string Description
        {
            get { return _description;  }
        }

        public static void ForceLoad()
        {
            // this function is used just for forcing loading the container assembly into app domain.
            if (!_forceLoaded)
            {
                _forceLoaded = true;
            }
        }
        public IEnumerable<ParameterAttribute> GetParameterDefinitions()
        {
            return _components.SelectMany(component => component.GetParameterDefinitions());
        }

        public void Initialize(IEvaluationContext context, IDictionary<Tuple<int, ParameterAttribute>, object> parameterValues)
        {
            if (context == null || parameterValues == null)
            {
                throw new ArgumentNullException();
            }

            for (int i = 0; i < _components.Length; ++i)
            {
                var componentParameterValues = parameterValues
                    .Where(kvp => kvp.Key.Item1 == i)
                    .ToDictionary(kvp => kvp.Key.Item2, kvp => kvp.Value);

                _components[i].Initialize(context, componentParameterValues);
            }

            _random = new Random(_globalSettings.RandomSeeds);

            _context = context;

            _context.GlobalSettings = _globalSettings;
        }

        public void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            throw new NotImplementedException();
        }

        public void WarmUp(ITradingObject tradingObject, Bar bar)
        {
            foreach (var component in _components)
            {
                component.WarmUp(tradingObject, bar);
            }
        }

        public void StartPeriod(DateTime time)
        {
            foreach (var component in _components)
            {
                component.StartPeriod(time);
            }

            _instructionsInCurrentPeriod = new List<Instruction>();
            _period = time;
        }

        public void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            throw new NotImplementedException();
        }

        public void Evaluate(ITradingObject[] tradingObjects, Bar[] bars)
        {
            if (tradingObjects == null || bars == null)
            {
                throw new ArgumentNullException();
            }

            if (tradingObjects.Length != bars.Length)
            {
                throw new ArgumentException("#trading object != #bars");
            }

            // evaluate all components. 
            // To improve cache hit rate, the outer loop is for component iteration
            // and the inner loop is for bars.
            foreach (var component in _components)
            {
                for (var i = 0; i < bars.Length; ++i)
                {
                    if (!tradingObjects[i].IsTradable)
                    {
                        continue;
                    }

                    if (bars[i].Time == Bar.InvalidTime)
                    {
                        continue;
                    }

                    component.EvaluateSingleObject(tradingObjects[i], bars[i]);
                }
            }

            // generate all possible instructions
            GenerateInstructions(tradingObjects, bars);

            // adjust instructions according to some limits
            AdjustInstructions();

            // sort instructions
            SortInstructions();
        }

        public bool EstimateStoplossAndSizeForNewPosition(Instruction instruction, double price, int totalNumberOfObjectsToBeEstimated)
        {
            OpenInstruction openInstruction = instruction as OpenInstruction;

            if (openInstruction == null)
            {
                return false;
            }

            if (Math.Abs(openInstruction.StopLossPriceForBuying) > 1e-6)
            {
                return true;
            }
            else if (Math.Abs(openInstruction.StopLossGapForBuying) > 1e-6)
            {
                openInstruction.StopLossPriceForBuying = price + openInstruction.StopLossGapForBuying;
                return true;
            }
            else
            {

                foreach (var filter in _buyPriceFilters)
                {
                    var filterResult = filter.IsPriceAcceptable(instruction.TradingObject, price);
                    if (!filterResult.IsPriceAcceptable)
                    {
                        openInstruction.Comments = string.Join(" ", instruction.Comments, filterResult.Comments);
                        openInstruction.Volume = 0;
                        openInstruction.StopLossGapForBuying = 0.0;
                        openInstruction.StopLossPriceForBuying = price;

                        return false;
                    }
                    else
                    {
                        price = filterResult.AcceptablePrice;
                    }
                }

                var stopLossResult = _stopLoss.EstimateStopLossGap(instruction.TradingObject, price);

                if (stopLossResult.StopLossGap > 0.0)
                {
                    throw new InvalidProgramException("the stop loss gap returned by the stop loss component is greater than zero");
                }

                if (!stopLossResult.IsStopLossReasonable)
                {
                    return false;
                }

                var stopLossGap = stopLossResult.StopLossGap;
                var positionSizingResult = _positionSizing.EstimatePositionSize(instruction.TradingObject, price, stopLossGap, totalNumberOfObjectsToBeEstimated);
                var volume = positionSizingResult.PositionSize;

                // adjust volume to ensure it fit the trading object's constraint
                volume -= volume % instruction.TradingObject.VolumePerBuyingUnit;

                openInstruction.Comments = string.Join(" ", instruction.Comments, stopLossResult.Comments);
                openInstruction.Comments = string.Join(" ", instruction.Comments, positionSizingResult.Comments);

                openInstruction.Volume = volume;

                openInstruction.StopLossGapForBuying = stopLossGap;

                openInstruction.StopLossPriceForBuying = price + stopLossGap;

                return true;
            }
        }

        public int GetMaxNewPositionCount(int totalNumberOfObjectsToBeEstimated)
        {
            return _positionSizing.GetMaxPositionCount(totalNumberOfObjectsToBeEstimated);
        }

        private void GenerateInstructions(ITradingObject[] tradingObjects, Bar[] bars)
        {
            // check if positions needs to be adjusted
            if (_positionAdjusting != null)
            {
                var instructions = _positionAdjusting.AdjustPositions();

                if (instructions != null)
                {
                    foreach (var instruction in instructions)
                    {
                        if (instruction.Action == TradingAction.OpenLong)
                        {
                            // comment out below code because evaluation result shows it will degrade performance.

                            //if (_globalSettings.AllowEnteringMarketOnlyWhenPriceIncreasing)
                            //{
                            //    var bar = bars[instruction.TradingObject.Index];
                            //    if (bar.ClosePrice <= bar.OpenPrice)
                            //    {
                            //        continue;
                            //    }
                            //}
                        }

                        _instructionsInCurrentPeriod.Add(instruction);
                    }
                }
            }

            for (var i = 0; i < tradingObjects.Length; ++i)
            {
                var tradingObject = tradingObjects[i];
                if (!tradingObject.IsTradable)
                {
                    continue;
                }

                var bar = bars[i];

                if (bar.Time == Bar.InvalidTime)
                {
                    continue;
                }

                Position[] positions;

                if (_context.ExistsPosition(tradingObject.Symbol))
                {
                    var temp = _context.GetPositionDetails(tradingObject.Symbol);
                    positions = temp as Position[] ?? temp.ToArray();
                }
                else
                {
                    positions = _emptyPositionArray;
                }

                // decide if we need to stop loss for some positions.
                bool isStopLost = false;
                List<CloseInstruction> stopLossInstructions = new List<CloseInstruction>();

                foreach (var position in positions)
                {
                    var stopLossPrice = double.MaxValue;

                    if (_globalSettings.StopLossByClosePrice)
                    {
                        if (position.StopLossPrice > bar.ClosePrice)
                        {
                            stopLossPrice = Math.Min(bar.ClosePrice, stopLossPrice);
                        }
                    }
                    else
                    {
                        if (position.StopLossPrice > bar.OpenPrice)
                        {
                            stopLossPrice = Math.Min(bar.OpenPrice, stopLossPrice);
                        }
                        else if (position.StopLossPrice > bar.LowestPrice)
                        {
                            stopLossPrice = Math.Min(position.StopLossPrice, stopLossPrice);
                        }
                    }

                    if (stopLossPrice < double.MaxValue)
                    {
                        TradingPrice price = new TradingPrice(
                            TradingPricePeriod.CurrentPeriod, 
                            TradingPriceOption.CustomPrice, 
                            stopLossPrice);

                        stopLossInstructions.Add(new CloseInstruction(_period, tradingObject, price)
                            {
                                Comments = string.Format("stop loss @{0:0.000}", stopLossPrice),
                                SellingType = SellingType.ByStopLossPrice,
                                StopLossPriceForSelling = stopLossPrice,
                                PositionIdForSell = position.Id,
                                Volume = position.Volume,
                            });

                        isStopLost = true;
                    }
                }

                // decide if we need to exit market for this trading object. 
                bool isExited = false;
                List<CloseInstruction> exitInstructions = new List<CloseInstruction>();
                if (positions.Any())
                {
                    foreach (var component in _marketExiting)
                    {
                        var marketExitingResult = component.ShouldExit(tradingObject);
                        if (marketExitingResult.ShouldExit)
                        {
                            exitInstructions.Add(new CloseInstruction(_period, tradingObject, marketExitingResult.Price)
                                {
                                    Comments = "Exiting market: " + marketExitingResult.Comments,
                                    SellingType = SellingType.ByVolume,
                                    Volume = positions.Sum(p => p.Volume),
                                });

                            isExited = true;
                        }
                    }
                }

                if (isStopLost || isExited)
                {

                    // merge stop loss instructions and exit instructions
                    var instructions = MergeStopLossAndExitInstructions(stopLossInstructions, exitInstructions);

                    if (instructions.Any())
                    {
                        _instructionsInCurrentPeriod.AddRange(instructions);
                    }

                    // if there is position to stop loss or exit for given trading object, 
                    // we will never consider entering market for the object.
                    continue;
                }

                // decide if we should enter market
                if (!positions.Any())
                {
                    if (!_globalSettings.AllowEnteringMarketOnlyWhenPriceIncreasing || bar.ClosePrice > bar.OpenPrice)
                    {
                        var allResults = new List<MarketEnteringComponentResult>();
                        bool canEnter = true;
                        foreach (var component in _marketEntering)
                        {
                            var marketEnteringResult = component.CanEnter(tradingObject);
                            if (!marketEnteringResult.CanEnter)
                            {
                                canEnter = false;
                                break;
                            }

                            allResults.Add(marketEnteringResult);
                        }

                        if (canEnter)
                        {
                            int nonDefaultPriceCount = allResults.Count(r => r.Price != null);
                            if (nonDefaultPriceCount > 1)
                            {
                                throw new InvalidOperationException("there are more than one non-default price for entering market");
                            }

                            TradingPrice price = nonDefaultPriceCount == 0 
                                ? null
                                : allResults.Where(r => r.Price != null).First().Price;

                            var comments = "Entering market: " + 
                                string.Join(";", allResults.Select(r => r.Comments).Where(c => c != null));

                            var relatedObjects = allResults.Select(r => r.RelatedObject).Where(r => r != null);

                            CreateInstructionForBuying(
                                tradingObject,
                                price,
                                comments,
                                relatedObjects.Count() == 0 ? null : relatedObjects.ToArray());

                            _context.DumpBarsFromCurrentPeriod(tradingObject);
                        }
                    }
                }
            }

            // update default price for instructions
            foreach (var instruction in _instructionsInCurrentPeriod)
            {
                _context.SetDefaultPriceForInstructionWhenNecessary(instruction);
            }
        }

        private List<CloseInstruction> MergeStopLossAndExitInstructions(
            IEnumerable<CloseInstruction> stopLossInstructions,
            IEnumerable<CloseInstruction> exitInstructions)
        {
            List<CloseInstruction> result = new List<CloseInstruction>();

            if (!stopLossInstructions.Any() && !exitInstructions.Any())
            {
                return result;
            }

            // first of all, we can only keep one exit instruction for current period 
            // and one exit instruction for next period.
            CloseInstruction exitInstructionForCurrentPeriod = null;
            CloseInstruction exitInstructionForNextPeriod = null;

            if (exitInstructions.Any())
            {
                foreach (var instruction in exitInstructions)
                {
                    _context.SetDefaultPriceForInstructionWhenNecessary(instruction);
                }

                var exitInstructionsForCurrentPeriod = exitInstructions.Where(i => i.Price.Period == TradingPricePeriod.CurrentPeriod).ToList();
                var exitInstructionsForNextPeriod = exitInstructions.Where(i => i.Price.Period == TradingPricePeriod.NextPeriod).ToList();

                if (exitInstructionsForCurrentPeriod.Any())
                {
                    exitInstructionForCurrentPeriod = exitInstructionsForCurrentPeriod
                        .OrderBy(i => i, new CloseInstructionPriceComparer())
                        .First();
                }

                if (exitInstructionsForNextPeriod.Any())
                {
                    exitInstructionForNextPeriod = exitInstructionsForNextPeriod
                        .OrderBy(i => i, new CloseInstructionPriceComparer())
                        .First();
                }
            }

            if (stopLossInstructions.Any())
            {
                // sort stop loss instructions
                var orderedStopLossInstructions = stopLossInstructions.OrderBy(i => i, new CloseInstructionPriceComparer());

                // stop loss instruction is always for current period, so we only care 
                // the exit instruction that is for current period
                if (exitInstructionForCurrentPeriod == null)
                {
                    result.AddRange(orderedStopLossInstructions);
                }
                else
                {
                    // for same object, if there are both stop loss and exit instruction, all stop loss instructions
                    // that price is higher than exit instruction should be removed.
                    CloseInstructionPriceComparer comparer = new CloseInstructionPriceComparer();
                    foreach (var stopLossInstruction in orderedStopLossInstructions)
                    {
                        if (comparer.Compare(stopLossInstruction, exitInstructionForCurrentPeriod) < 0)
                        {
                            result.Add(stopLossInstruction);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (exitInstructionForCurrentPeriod != null)
            {
                result.Add(exitInstructionForCurrentPeriod);
            }

            if (exitInstructionForNextPeriod != null)
            {
                result.Add(exitInstructionForNextPeriod);
            }

            return result;
        }

        private IEnumerable<Instruction> SortInstructions(IEnumerable<Instruction> instructions, InstructionSortMode mode, RuntimeMetricProxy metricProxy)
        {
            switch (mode)
            {
                case InstructionSortMode.NoSorting:
                    return instructions;

                case InstructionSortMode.Randomize:
                    var randomizedInstructions = instructions.OrderBy(instruction => _random.Next());
                    return randomizedInstructions;

                case InstructionSortMode.SortBySymbolAscending:
                    return instructions.OrderBy(instruction => instruction.TradingObject.Symbol);

                case InstructionSortMode.SortBySymbolDescending:
                    return instructions.OrderBy(instruction => instruction.TradingObject.Symbol).Reverse();

                case InstructionSortMode.SortByInstructionIdAscending:
                    return instructions.OrderBy(instruction => instruction.Id);

                case InstructionSortMode.SortByInstructionIdDescending:
                    return instructions.OrderBy(instruction => instruction.Id).Reverse();

                case InstructionSortMode.SortByVolumeAscending:
                    return instructions.OrderBy(instruction => instruction.Volume);

                case InstructionSortMode.SortByVolumeDescending:
                    return instructions.OrderBy(instruction => -instruction.Volume);

                case InstructionSortMode.SortByMetricAscending:
                    return instructions.OrderBy(
                        instruction => metricProxy.GetMetricValues(instruction.TradingObject)[0]);
                    
                case InstructionSortMode.SortByMetricDescending:
                    return instructions.OrderBy(
                        instruction => -metricProxy.GetMetricValues(instruction.TradingObject)[0]);

                default:
                    throw new NotSupportedException(string.Format("unsupported instruction sort mode {0}", mode));
            }
        }

        private void SortInstructions()
        {
            var closeLongInstructions = _instructionsInCurrentPeriod
                .Where(instruction => instruction.Action == TradingAction.CloseLong)
                .ToList();

            var IncreasePositionInstructions = _instructionsInCurrentPeriod
                .Where(instruction => instruction.Action == TradingAction.OpenLong
                     && _context.ExistsPosition(instruction.TradingObject.Symbol))
                .ToList();

            var NewPositionInstructions = _instructionsInCurrentPeriod
                .Where(instruction => instruction.Action == TradingAction.OpenLong
                    && !_context.ExistsPosition(instruction.TradingObject.Symbol))
                .ToList();

            // sort instructions
            IncreasePositionInstructions = 
                SortInstructions(
                    IncreasePositionInstructions, 
                    _globalSettings.IncreasePositionInstructionSortMode,
                    _globalSettings.IncreasePositionSortMetricProxy)
                .ToList();

            NewPositionInstructions =
                SortInstructions(
                    NewPositionInstructions, 
                    _globalSettings.NewPositionInstructionSortMode,
                    _globalSettings.NewPositionSortMetricProxy)
                .ToList(); 

            // reconstruct instructions in current period
            _instructionsInCurrentPeriod = new List<Instruction>();

            if (_globalSettings.CloseInstructionFirst)
            {
                _instructionsInCurrentPeriod.AddRange(closeLongInstructions);
            }

            switch(_globalSettings.InstructionOrder)
            { 
                case OpenPositionInstructionOrder.IncPosThenNewPos:
                    _instructionsInCurrentPeriod.AddRange(IncreasePositionInstructions);
                    _instructionsInCurrentPeriod.AddRange(NewPositionInstructions);
                    break;
                case OpenPositionInstructionOrder.NewPosThenIncPos:
                    _instructionsInCurrentPeriod.AddRange(NewPositionInstructions);
                    _instructionsInCurrentPeriod.AddRange(IncreasePositionInstructions);
                    break;
                default:
                    throw new NotImplementedException(string.Format("unsupported instruction order {0}", _globalSettings.InstructionOrder));
            }

            if (!_globalSettings.CloseInstructionFirst)
            {
                _instructionsInCurrentPeriod.AddRange(closeLongInstructions);
            }
        }

        private void AdjustInstructions()
        {
            // it is possible the open long instruction conflicts with close long instruction, and we always put close long as top priority
            var closeLongSymbols = _instructionsInCurrentPeriod
                .Where(instruction => instruction.Action == TradingAction.CloseLong)
                .Select(instruction => instruction.TradingObject.Symbol)
                .GroupBy(symbol => symbol)
                .Select(g => g.Key)
                .ToDictionary(symbol => symbol);

            _instructionsInCurrentPeriod = _instructionsInCurrentPeriod
                .Where(instruction => instruction.Action == TradingAction.CloseLong 
                    || (instruction.Action == TradingAction.OpenLong 
                        && !closeLongSymbols.ContainsKey(instruction.TradingObject.Symbol)))
                .ToList();

            // randomly remove instruction
            if (_allowRemovingInstructionRandomly && _globalSettings.RandomlyRemoveInstruction)
            {
                var openLongInstructions = _instructionsInCurrentPeriod
                    .Where(instruction => instruction.Action == TradingAction.OpenLong)
                    .ToList();

                if (openLongInstructions.Count() > 0)
                {
                    int pos = 0;
                    for (int i = 0; i < openLongInstructions.Count(); ++i)
                    {
                        if (_random.Next(100) < _globalSettings.RandomlyRemoveInstructionThreshold)
                        {
                            openLongInstructions.RemoveAt(pos);
                        }
                        else
                        {
                            ++pos;
                        }
                    }

                    _instructionsInCurrentPeriod = _instructionsInCurrentPeriod
                        .Where(instruction => instruction.Action == TradingAction.CloseLong)
                        .Union(openLongInstructions)
                        .ToList();
                }
            }
        }

        private void CreateInstructionForBuying(ITradingObject tradingObject, TradingPrice price, string comments, object[] relatedObjects)
        {
                _instructionsInCurrentPeriod.Add(
                    new OpenInstruction(_period, tradingObject, price)
                    {
                        Comments = comments,
                        Volume = 0, // will be filled in EstimateStoplossAndSizeOfNewPosition()
                        StopLossGapForBuying = 0.0, // will be filled in EstimateStoplossAndSizeOfNewPosition()
                        StopLossPriceForBuying = 0.0, // will be filled in EstimateStoplossAndSizeOfNewPosition()
                        RelatedObjects = relatedObjects
                    });
        }

        public void NotifyTransactionStatus(Transaction transaction)
        {
            Instruction instruction;
            if (!_activeInstructions.TryGetValue(transaction.InstructionId, out instruction))
            {
                throw new InvalidOperationException(
                    string.Format("can't find instruction {0} associated with the transaction.", transaction.InstructionId));
            }

            if (transaction.Action == TradingAction.OpenLong)
            {
                if (transaction.Succeeded)
                {
                    // update the stop loss and risk for new positions
                    var symbol = transaction.Symbol;
                    if (!_context.ExistsPosition(symbol))
                    {
                        throw new InvalidOperationException(
                            string.Format("There is no position for {0} when calling this function", symbol));
                    }

                    var positions = _context.GetPositionDetails(symbol);

                    if (!positions.Any())
                    {
                        throw new InvalidProgramException("Logic error");
                    }

                    OpenInstruction openInstruction = instruction as OpenInstruction;

                    // set stop loss and initial risk for all new positions
                    if (positions.Count() == 1)
                    {
                        var position = positions.Last();
                        if (!position.IsStopLossPriceInitialized())
                        {
                            position.SetStopLossPrice(openInstruction.StopLossPriceForBuying);

                            _context.Log(
                                string.Format(
                                    "Set stop loss for position {0}/{1} as {2:0.000}",
                                    position.Id,
                                    position.Symbol,
                                    openInstruction.StopLossPriceForBuying));
                        }
                    }
                    else
                    {
                        // set stop loss for positions created by PositionAdjusting component
                        if (Math.Abs(openInstruction.StopLossPriceForBuying) > 1e-6)
                        {
                            var lastPosition = positions.Last();
                            var newStopLossPrice = openInstruction.StopLossPriceForBuying;
                                
                            // now set the new stop loss price for all positions
                            foreach (var position in positions)
                            {
                                if (!position.IsStopLossPriceInitialized()
                                    || position.StopLossPrice < newStopLossPrice)
                                {
                                    position.SetStopLossPrice(newStopLossPrice);

                                    _context.Log(
                                        string.Format(
                                            "PositionAdjusting:IncreaseStopLoss: Set stop loss for position {0}/{1} as {2:0.000}",
                                            position.Id,
                                            position.Symbol,
                                            newStopLossPrice));
                                }
                            }
                        }
                    }
                }
            }

            // remove the instruction from active instruction collection.
            _activeInstructions.Remove(instruction.Id);
        }

        public IEnumerable<Instruction> RetrieveInstructions()
        {
            if (_instructionsInCurrentPeriod != null)
            {
                var temp = _instructionsInCurrentPeriod;

                foreach (var instruction in _instructionsInCurrentPeriod)
                {
                    _activeInstructions.Add(instruction.Id, instruction);
                }

                _instructionsInCurrentPeriod = null;

                return temp;
            }
            return null;
        }

        public void EndPeriod()
        {
            foreach (var component in _components)
            {
                component.EndPeriod();
            }

            _instructionsInCurrentPeriod = null;
        }

        public void Finish()
        {
            foreach (var component in _components)
            {
                component.Finish();
            }

            if (_activeInstructions.Count > 0)
            {
                foreach (var id in _activeInstructions.Keys)
                {
                    _context.Log(string.Format("unexecuted instruction {0}.", id));
                }
            }
        }

        public CombinedStrategy(ITradingStrategyComponent[] components, bool allowRemovingInstructionRandomly)
        {
            if (components == null || !components.Any())
            {
                throw new ArgumentNullException();
            }

            _components = components;
            _allowRemovingInstructionRandomly = allowRemovingInstructionRandomly;

            foreach (var component in components)
            {
                if (component is GlobalSettingsComponent)
                {
                    SetComponent(component, ref _globalSettings);
                }

                if (component is IPositionSizingComponent)
                {
                    SetComponent(component, ref _positionSizing);
                }
                
                if (component is IMarketEnteringComponent)
                {
                    _marketEntering.Add((IMarketEnteringComponent)component);
                }

                if (component is IMarketExitingComponent)
                {
                    _marketExiting.Add((IMarketExitingComponent)component);
                }
                
                if (component is IBuyPriceFilteringComponent)
                {
                    _buyPriceFilters.Add((IBuyPriceFilteringComponent)component);
                }

                if (component is IStopLossComponent)
                {
                    SetComponent(component, ref _stopLoss);
                }

                if (component is IPositionAdjustingComponent)
                {
                    SetComponent(component, ref _positionAdjusting);
                }
            }

            // PositionAdjusting and BuyPriceFiltering component could be null
            if (_positionSizing == null
                || _marketExiting.Count == 0
                || _marketEntering.Count == 0
                || _stopLoss == null
                || _globalSettings == null)
            {
                throw new ArgumentException("Missing at least one type of component");
            }

            _name = "复合策略，包含以下组件：\n";
            _name += string.Join(Environment.NewLine, _components.Select(c => c.Name));

            _description = "复合策略，包含以下组件描述：\n";
            _description += string.Join(Environment.NewLine, _components.Select(c => c.Description));
        }

        private static void SetComponent<T>(ITradingStrategyComponent component, ref T obj)
        {
            if (component == null)
            {
                throw new ArgumentNullException();
            }

            if (component is T)
            {
// ReSharper disable CompareNonConstrainedGenericWithNull
                if (obj == null)
// ReSharper restore CompareNonConstrainedGenericWithNull
                {
                    obj = (T)component;
                }
                else
                {
                    throw new ArgumentException(string.Format("Duplicated {0} objects", typeof(T)));
                }
            }
            else
            {
                throw new ArgumentException(string.Format("unmatched component type {0}", typeof(T)));
            }
        }
    }
}
