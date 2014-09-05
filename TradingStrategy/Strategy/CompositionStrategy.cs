using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class CompositionStrategy : ITradingStrategy
    {
        private ITradingStrategyComponent[] _components = null;
        private IPositionSizingComponent _positionSizing = null;
        private IMarketEnteringComponent _marketEntering = null;
        private IMarketExitingComponent _marketExiting = null;
        private IStopLossComponent _stopLoss = null;

        private string _name;
        private string _description;

        private IEvaluationContext _context;
        private List<Instruction> _instructions;
        private DateTime _period;
        private Dictionary<ITradingObject, Bar> _barsInPeriod;
        private Dictionary<string, ITradingObject> _codeToTradingObjectMap;

        public string Name
        {
            get { return _name; }
        }

        public string Description
        {
            get { return _description;  }
        }

        public IEnumerable<ParameterAttribute> GetParameterDefinitions()
        {
            foreach (var component in _components)
            {
                foreach (var attribute in component.GetParameterDefinitions())
                {
                    yield return attribute;
                }
            }
        }

        public void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            foreach (var component in _components)
            {
                component.Initialize(context, parameterValues);
            }

            _context = context;
        }

        public void WarmUp(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
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

            _instructions = new List<Instruction>();
            _barsInPeriod = new Dictionary<ITradingObject, Bar>();
            _codeToTradingObjectMap = new Dictionary<string, ITradingObject>();
            _period = time;
        }

        public void Evaluate(ITradingObject tradingObject, Bar bar)
        {
            if (bar.Invalid())
            {
                return;
            }

            // remember the trading object and bar even if bar is invalid
            // because the object could be used in AfterEvaulation
            _barsInPeriod.Add(tradingObject, bar);
            _codeToTradingObjectMap.Add(tradingObject.Code, tradingObject);
            foreach (var component in _components)
            {
                component.Evaluate(tradingObject, bar);
            }

            string comments;
            var positions = _context.ExistsPosition(tradingObject.Code)
                ? _context.GetPositionDetails(tradingObject.Code)
                : (IEnumerable<Position>)new List<Position>();

            // decide if we need to exit market for this trading object. This is the first priorty work
            if (positions.Count() > 0)
            {
                if (_marketExiting.ShouldExit(tradingObject, out comments))
                {
                    _instructions.Add(
                        new Instruction()
                        {
                            Action = TradingAction.CloseLong,
                            Comments = "market exiting condition triggered. " + comments,
                            ID = _context.GetUniqueInstructionId(),
                            Payload = null,
                            SubmissionTime = _period,
                            TradingObject = tradingObject,
                            Volume = positions.Sum(p => p.Volume),
                            StopLossPriceForSell = double.MinValue,
                        });

                    return;
                }
            }

            // decide if we need to stop loss for some positions
            double maxStopLossPrice = double.MinValue;
            int totalVolume = 0;
            foreach (var position in positions)
            {
                if (position.StopLossPrice >= maxStopLossPrice)
                {
                    maxStopLossPrice = position.StopLossPrice;
                }
                else
                {
                    throw new InvalidOperationException("positions' stop loss price is decreasing");
                }

                if (position.StopLossPrice > bar.ClosePrice)
                {
                    totalVolume += position.Volume;
                }
            }

            if (totalVolume > 0)
            {
                _instructions.Add(
                    new Instruction()
                    {
                        Action = TradingAction.CloseLong,
                        Comments = string.Format("stop loss @{0:0.000}", bar.ClosePrice),
                        ID = _context.GetUniqueInstructionId(),
                        Payload = null,
                        SubmissionTime = _period,
                        TradingObject = tradingObject,
                        Volume = totalVolume,
                        StopLossPriceForSell = bar.ClosePrice
                    });

                return;
            }

            // decide if we should enter market
            if (positions.Count() == 0 
                && _marketEntering.CanEnter(tradingObject, out comments))
            {
                object payload;
                double stopLossGap = _stopLoss.EstimateStopLossGap(tradingObject, bar.ClosePrice, out payload);
                int volume = _positionSizing.EstimatePositionSize(tradingObject, bar.ClosePrice, stopLossGap);

                _instructions.Add(
                    new Instruction()
                    {
                        Action = TradingAction.OpenLong,
                        Comments = "Entering market. " + comments,
                        ID = _context.GetUniqueInstructionId(),
                        Payload = null,
                        StopLossPriceForSell = double.NaN,
                        SubmissionTime = _period,
                        TradingObject = tradingObject,
                        Volume = volume
                    });
            }
        }

        public void AfterEvaluation()
        {
            // decide if existing position should be adjusted
            string[] codesForAddingPosition;
            string[] codesForRemovingPosition;

            if (_positionSizing.ShouldAdjustPosition(out codesForAddingPosition, out codesForRemovingPosition))
            {
                if (codesForRemovingPosition != null
                    && codesForRemovingPosition.Length > 0)
                {
                    // remove positions
                    foreach (var code in codesForRemovingPosition)
                    {
                        if (!_context.ExistsPosition(code))
                        {
                            throw new InvalidOperationException("There is no position for code " + code);
                        }

                        ITradingObject tradingObject;
                        
                        if (!_codeToTradingObjectMap.TryGetValue(code, out tradingObject))
                        {
                            // ignore the request of removing position because the trading object has 
                            // no valid bar this period.
                            continue;
                        }

                        var positions = _context.GetPositionDetails(code);

                        _instructions.Add(
                            new Instruction()
                            {
                                Action = TradingAction.CloseLong,
                                Comments = "adjust position triggered. ",
                                ID = _context.GetUniqueInstructionId(),
                                Payload = null,
                                SubmissionTime = _period,
                                TradingObject = tradingObject,
                                Volume = positions.Sum(p => p.Volume),
                                StopLossPriceForSell = double.MinValue,
                            });
                    }
                }
                else if (codesForAddingPosition != null
                    && codesForAddingPosition.Length > 0)
                {
                    // adding positions
                    foreach (var code in codesForAddingPosition)
                    {
                        if (!_context.ExistsPosition(code))
                        {
                            throw new InvalidOperationException("There is no position for code " + code);
                        }

                        ITradingObject tradingObject;

                        if (!_codeToTradingObjectMap.TryGetValue(code, out tradingObject))
                        {
                            // ignore the request of adding position because the trading object has 
                            // no valid bar this period.
                            continue;
                        }

                        Bar bar = _barsInPeriod[tradingObject];

                        object payload;
                        double stopLossGap = _stopLoss.EstimateStopLossGap(tradingObject, bar.ClosePrice, out payload);
                        int volume = _positionSizing.EstimatePositionSize(tradingObject, bar.ClosePrice, stopLossGap);

                        _instructions.Add(
                            new Instruction()
                            {
                                Action = TradingAction.OpenLong,
                                Comments = "Adding position. ",
                                ID = _context.GetUniqueInstructionId(),
                                Payload = null,
                                StopLossPriceForSell = double.NaN,
                                SubmissionTime = _period,
                                TradingObject = tradingObject,
                                Volume = volume
                            });
                    }
                }

            }
        }

        public void NotifyTransactionStatus(Transaction transaction)
        {
            if (transaction.Succeeded)
            {
                Instruction instruction = _instructions.Find(i => i.ID == transaction.InstructionId);

                if (instruction == null)
                {
                    throw new InvalidProgramException("can't find instruction id specified in transaction");
                }

                _stopLoss.UpdateStopLoss(instruction.TradingObject, instruction.Payload);
            }
        }

        public IEnumerable<Instruction> GetInstructions()
        {
            return _instructions;
        }

        public void EndPeriod()
        {
            foreach (var component in _components)
            {
                component.EndPeriod();
            }

            _instructions = null;
            _barsInPeriod = null;
        }

        public void Finish()
        {
            foreach (var component in _components)
            {
                component.Finish();
            }
        }

        public CompositionStrategy(IEnumerable<ITradingStrategyComponent> components)
        {
            if (components == null || components.Count() == 0)
            {
                throw new ArgumentNullException();
            }

            _components = components.ToArray();

            foreach (var component in components)
            {
                if (component is IPositionSizingComponent)
                {
                    SetComponent(component, ref _positionSizing);
                }
                else if (component is IMarketEnteringComponent)
                {
                    SetComponent(component, ref _marketEntering);
                }
                else if (component is IMarketExitingComponent)
                {
                    SetComponent(component, ref _marketExiting);
                }
                else if (component is IStopLossComponent)
                {
                    SetComponent(component, ref _stopLoss);
                }
                else
                {
                    throw new ArgumentException(string.Format("unsupported component type {0}", component.GetType()));
                }
            }

            if (_positionSizing == null
                || _marketExiting == null
                || _marketEntering == null
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
                if (obj == null)
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
