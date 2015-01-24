using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class CombinedStrategy : ITradingStrategy
    {
        private static bool _forceLoaded;

        private readonly ITradingStrategyComponent[] _components;
        private readonly IPositionSizingComponent _positionSizing;
        private readonly List<IMarketEnteringComponent> _marketEntering = new List<IMarketEnteringComponent>();
        private readonly List<IMarketExitingComponent> _marketExiting = new List<IMarketExitingComponent>();
        private readonly IStopLossComponent _stopLoss;
        private readonly IPositionAdjustingComponent _positionAdjusting;
        private readonly GlobalSettingsComponent _globalSettings;

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

        public void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            foreach (var component in _components)
            {
                component.Initialize(context, parameterValues);
            }

            _random = new Random(_globalSettings.RandomSeeds);

            _context = context;
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

            // evaluate all components
            foreach (var component in _components)
            {
                for (var i = 0; i < bars.Length; ++i)
                {
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

        private void GenerateInstructions(ITradingObject[] tradingObjects, Bar[] bars)
        {
            // check if positions needs to be adjusted
            if (_positionAdjusting != null)
            {
                var instructions = _positionAdjusting.AdjustPositions();

                if (instructions != null)
                {
                    _instructionsInCurrentPeriod.AddRange(instructions);
                }
            }

            for (var i = 0; i < tradingObjects.Length; ++i)
            {
                var tradingObject = tradingObjects[i];
                var bar = bars[i];

                if (bar.Time == Bar.InvalidTime)
                {
                    continue;
                }

                Position[] positions;

                if (_context.ExistsPosition(tradingObject.Code))
                {
                    var temp = _context.GetPositionDetails(tradingObject.Code);
                    positions = temp as Position[] ?? temp.ToArray();
                }
                else
                {
                    positions = new Position[0];
                }

                // decide if we need to exit market for this trading object. This is the first priorty work
                if (positions.Any())
                {
                    bool exited = false;
                    foreach (var component in _marketExiting)
                    {
                        string comments;
                        if (component.ShouldExit(tradingObject, out comments))
                        {
                            _instructionsInCurrentPeriod.Add(
                                new Instruction
                                {
                                    Action = TradingAction.CloseLong,
                                    Comments = "Exiting market: " + comments,
                                    SubmissionTime = _period,
                                    TradingObject = tradingObject,
                                    SellingType = SellingType.ByVolume,
                                    Volume = positions.Sum(p => p.Volume),
                                });

                            exited = true;
                            break;
                        }
                    }

                    if (exited)
                    {
                        continue;
                    }
                }

                // decide if we need to stop loss for some positions
                var totalVolume = positions
                    .Where(position => position.StopLossPrice > bar.ClosePrice)
                    .Sum(position => position.Volume);

                if (totalVolume > 0)
                {
                    _instructionsInCurrentPeriod.Add(
                        new Instruction
                        {
                            Action = TradingAction.CloseLong,
                            Comments = string.Format("stop loss @{0:0.000}", bar.ClosePrice),
                            SubmissionTime = _period,
                            TradingObject = tradingObject,
                            SellingType = SellingType.ByStopLossPrice,
                            StopLossPriceForSelling = bar.ClosePrice,
                            Volume = totalVolume
                        });

                    continue;
                }

                // decide if we should enter market
                if (!positions.Any())
                {
                    if (!_globalSettings.AllowEnteringMarketOnlyWhenPriceIncreasing || bar.ClosePrice > bar.OpenPrice)
                    {
                        var allComments = new List<string>(_marketEntering.Count + 1);
                        List<object> objectsForEntering = new List<object>();

                        var canEnter = true;
                        foreach (var component in _marketEntering)
                        {
                            string subComments;
                            object obj;

                            if (!component.CanEnter(tradingObject, out subComments, out obj))
                            {
                                canEnter = false;
                                break;
                            }

                            allComments.Add(subComments);

                            if (obj != null)
                            {
                                objectsForEntering.Add(obj);
                            }
                        }

                        if (canEnter)
                        {
                            CreateIntructionForBuying(
                                tradingObject,
                                bar.ClosePrice,
                                "Entering market: " + string.Join(";", allComments),
                                objectsForEntering.Count > 0 ? objectsForEntering.ToArray() : null);
                        }
                    }
                }
            }
        }

        private IEnumerable<Instruction> SortInstructions(IEnumerable<Instruction> instructions, InstructionSortMode mode, int metricIndex)
        {
            switch (mode)
            {
                case InstructionSortMode.NoSorting:
                    return instructions;

                case InstructionSortMode.Randomize:
                    return instructions.OrderBy(instruction => _random.Next());

                case InstructionSortMode.SortByCodeAscending:
                    return instructions.OrderBy(instruction => instruction.TradingObject.Code);

                case InstructionSortMode.SortByCodeDescending:
                    return instructions.OrderBy(instruction => instruction.TradingObject.Code).Reverse();

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
                        instruction => _context.MetricManager.GetMetricValues(instruction.TradingObject, metricIndex)[0]);
                    
                case InstructionSortMode.SortByMetricDescending:
                    return instructions.OrderBy(
                        instruction => -_context.MetricManager.GetMetricValues(instruction.TradingObject, metricIndex)[0]);

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
                     && _context.ExistsPosition(instruction.TradingObject.Code))
                .ToList();

            var NewPositionInstructions = _instructionsInCurrentPeriod
                .Where(instruction => instruction.Action == TradingAction.OpenLong
                    && !_context.ExistsPosition(instruction.TradingObject.Code))
                .ToList();

            // sort instructions
            IncreasePositionInstructions = 
                SortInstructions(
                    IncreasePositionInstructions, 
                    _globalSettings.IncreasePositionInstructionSortMode,
                    _globalSettings.IncreasePositionSortMetricIndex)
                .ToList();

            NewPositionInstructions =
                SortInstructions(
                    NewPositionInstructions, 
                    _globalSettings.NewPositionInstructionSortMode,
                    _globalSettings.NewPositionSortMetricIndex)
                .ToList(); 

            // reconstruct instructions in current period
            _instructionsInCurrentPeriod = new List<Instruction>();
            _instructionsInCurrentPeriod.AddRange(closeLongInstructions);

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
        }

        private void AdjustInstructions()
        {
            // it is possible the open long instruction conflicts with close long instruction, and we always put close long as top priority
            var closeLongCodes = _instructionsInCurrentPeriod
                .Where(instruction => instruction.Action == TradingAction.CloseLong)
                .Select(instruction => instruction.TradingObject.Code)
                .ToDictionary(code => code);

            _instructionsInCurrentPeriod = _instructionsInCurrentPeriod
                .Where(instruction => instruction.Action == TradingAction.CloseLong 
                    || (instruction.Action == TradingAction.OpenLong 
                        && !closeLongCodes.ContainsKey(instruction.TradingObject.Code)))
                .ToList();
        }

        private void CreateIntructionForBuying(ITradingObject tradingObject, double price, string comments, object[] relatedObjects)
        {
            string stopLossComments;
            var stopLossGap = _stopLoss.EstimateStopLossGap(tradingObject, price, out stopLossComments);
            if (stopLossGap >= 0.0)
            {
                throw new InvalidProgramException("the stop loss gap returned by the stop loss component is greater than zero");
            }

            string positionSizeComments;
            var volume = _positionSizing.EstimatePositionSize(tradingObject, price, stopLossGap, out positionSizeComments);

            // adjust volume to ensure it fit the trading object's constraint
            volume -= volume % tradingObject.VolumePerBuyingUnit;

            if (volume > 0)
            {
                _instructionsInCurrentPeriod.Add(
                    new Instruction
                    {
                        Action = TradingAction.OpenLong,
                        Comments = string.Join(" ", comments, stopLossComments, positionSizeComments),
                        SubmissionTime = _period,
                        TradingObject = tradingObject,
                        Volume = volume,
                        StopLossGapForBuying = stopLossGap,
                        RelatedObjects = relatedObjects
                    });
            }
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
                    var code = transaction.Code;
                    if (!_context.ExistsPosition(code))
                    {
                        throw new InvalidOperationException(
                            string.Format("There is no position for {0} when calling this function", code));
                    }

                    var positions = _context.GetPositionDetails(code);

                    if (!positions.Any())
                    {
                        throw new InvalidProgramException("Logic error");
                    }

                    // set stop loss and initial risk for all new positions
                    if (positions.Count() == 1)
                    {
                        var position = positions.First();
                        if (!position.IsStopLossPriceInitialized())
                        {
                            string comments;

                            var stopLossGap = _stopLoss.EstimateStopLossGap(instruction.TradingObject, position.BuyPrice, out comments);

                            var stopLossPrice = Math.Max(0.0, position.BuyPrice + stopLossGap);

                            position.SetStopLossPrice(stopLossPrice);

                            _context.Log(
                                string.Format(
                                    "Set stop loss for position {0}/{1} as {2:0.000}",
                                    position.Id,
                                    position.Code,
                                    stopLossPrice));
                        }
                    }
                    else
                    {
                        // set stop loss for positions created by PositionAdjusting component
                        if (Math.Abs(instruction.StopLossGapForBuying) > 1e-16)
                        {
                            var lastPosition = positions.Last();
                            var newStopLossPrice = instruction.StopLossGapForBuying +
                                (transaction.Succeeded ? lastPosition.BuyPrice : transaction.Price);

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
                                            position.Code,
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

        public CombinedStrategy(
            ITradingStrategyComponent[] components)
        {
            if (components == null || !components.Any())
            {
                throw new ArgumentNullException();
            }

            _components = components;

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
                
                if (component is IStopLossComponent)
                {
                    SetComponent(component, ref _stopLoss);
                }

                if (component is IPositionAdjustingComponent)
                {
                    SetComponent(component, ref _positionAdjusting);
                }
            }

            // PositionAdjusting component could be null
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
