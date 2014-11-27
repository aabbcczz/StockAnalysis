using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class CombinedStrategy : ITradingStrategy
    {
        private int _maxNumberOfActiveStocks = 1000;
        private int _maxNumberOfActiveStocksPerBlock = 100;

        private static bool _forceLoaded;

        private readonly ITradingStrategyComponent[] _components;
        private readonly IPositionSizingComponent _positionSizing;
        private readonly List<IMarketEnteringComponent> _marketEntering = new List<IMarketEnteringComponent>();
        private readonly List<IMarketExitingComponent> _marketExiting = new List<IMarketExitingComponent>();
        private readonly IStopLossComponent _stopLoss;
        private readonly IPositionAdjustingComponent _positionAdjusting;

        private readonly string _name;
        private readonly string _description;

        private IEvaluationContext _context;
        private List<Instruction> _instructionsInCurrentPeriod;
        private readonly Dictionary<long, Instruction> _activeInstructions = new Dictionary<long, Instruction>();
        private DateTime _period;

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
                            StopLossPriceForSell = bar.ClosePrice,
                            Volume = totalVolume
                        });

                    continue;
                }

                // decide if we should enter market
                if (!positions.Any())
                {
                    var allComments = new List<string>(_marketEntering.Count + 1);

                    var canEnter = true;
                    foreach (var component in _marketEntering)
                    {
                        string subComments;

                        if (!component.CanEnter(tradingObject, out subComments))
                        {
                            canEnter = false;
                            break;
                        }

                        allComments.Add(subComments);
                    }

                    if (canEnter)
                    {
                        CreateIntructionForBuying(tradingObject, bar.ClosePrice, "Entering market: " + string.Join(";", allComments));
                    }
                }
            }

            // for diversifying, limit total stocks and the number of stocks in the same stock
            var existingCodes = _context.GetAllPositionCodes().ToList();

            var codesToBeRemoved = _instructionsInCurrentPeriod
                .Where(instruction => instruction.Action == TradingAction.CloseLong)
                .Select(instruction => instruction.TradingObject.Code)
                .ToList();

            var codesToBeAdded = _instructionsInCurrentPeriod
                .Where(instruction => instruction.Action == TradingAction.OpenLong)
                .Select(instruction => instruction.TradingObject.Code)
                .ToList();

            var codesCannotBeAdded = new List<string>();

            var codesAfterRemoved = existingCodes.Except(codesToBeRemoved).ToList();

            // ensure each block has no too much positions
            if (_context.RelationshipManager != null)
            {
                var blockSizes = codesAfterRemoved
                    .SelectMany(code => _context.RelationshipManager.GetBlocksForStock(code))
                    .GroupBy(code => code)
                    .ToDictionary(g => g.Key, g => g.Count());

                foreach (var code in codesToBeAdded)
                {
                    foreach (var block in _context.RelationshipManager.GetBlocksForStock(code))
                    {
                        if (blockSizes.ContainsKey(block))
                        {
                            if (blockSizes[block] >= _maxNumberOfActiveStocksPerBlock)
                            {
                                // can't add
                                codesCannotBeAdded.Add(code);
                                continue;
                            }
                            blockSizes[block] = blockSizes[block] + 1;
                        }
                        else
                        {
                            blockSizes[block] = 1;
                        }
                    }
                }
            }

            // now check the overall number of stocks in active position
            codesToBeAdded = codesToBeAdded.Except(codesCannotBeAdded).ToList();

            if (codesAfterRemoved.Count + codesToBeAdded.Count > _maxNumberOfActiveStocks)
            {
                // need to remove some. 
                int keptCount = Math.Max(0, _maxNumberOfActiveStocks - codesAfterRemoved.Count);

                codesCannotBeAdded.AddRange(codesToBeAdded.Skip(keptCount));
            }

            // now remove all instructions about stock code in codesCannotBeAdded.
            var instructionsToBeRemoved = _instructionsInCurrentPeriod
                .Where(instruction => codesCannotBeAdded.Contains(instruction.TradingObject.Code))
                .ToList();

            foreach (var instruction in instructionsToBeRemoved)
            {
                _instructionsInCurrentPeriod.Remove(instruction);
            }
        }

        private void CreateIntructionForBuying(ITradingObject tradingObject, double price, string comments)
        {
            string stopLossComments;
            var stopLossGap = _stopLoss.EstimateStopLossGap(tradingObject, price, out stopLossComments);
            if (stopLossGap >= 0.0)
            {
                throw new InvalidProgramException("the stop loss gap returned by the stop loss component is greater than zero");
            }

            string positionSizeComments;
            var volume = _positionSizing.EstimatePositionSize(tradingObject, price, stopLossGap, out positionSizeComments);

            // add global contraint that no any stock can exceeds 5% of total capital
            // volume = Math.Min(volume, (int)(_context.GetInitialEquity() / 20.0 / price));

            // adjust volume to ensure it fit the trading object's contraint
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
                        Volume = volume
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

            if (transaction.Succeeded && transaction.Action == TradingAction.OpenLong)
            {
                // update the stop loss and risk for new positions
                var code = transaction.Code;
                if (!_context.ExistsPosition(code))
                {
                    throw new InvalidOperationException(
                        string.Format("There is no position for {0} when calling this function", code));
                }

                var positions = _context.GetPositionDetails(code);

                // set stop loss and initial risk for all new positions
                foreach (var position in positions)
                {
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
            ITradingStrategyComponent[] components, 
            int maxNumberOfActiveStocks,
            int maxNumberOfActiveStocksPerBlock)
        {
            if (components == null || !components.Any())
            {
                throw new ArgumentNullException();
            }

            _components = components;

            foreach (var component in components)
            {
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
                || _stopLoss == null)
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
