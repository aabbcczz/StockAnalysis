using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
using MetricsDefinition;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageStrategy : ITradingStrategy
    {
        private class RuntimeMetrics
        {
            public double Atr { get; private set; }

            public double ShortMA { get; private set; }

            public double LongMA { get; private set; }

            private MovingAverage _short;
            private MovingAverage _long;
            private AverageTrueRange _atr;

            public RuntimeMetrics(int shortPeriod, int longPeriod)
            {
                _short = new MovingAverage(shortPeriod);
                _long = new MovingAverage(longPeriod);
                _atr = new AverageTrueRange(longPeriod);
            }

            public void Update(Bar bar)
            {
                ShortMA = _short.Update(bar.ClosePrice);
                LongMA = _long.Update(bar.ClosePrice);
                Atr = _atr.Update(bar);
            }
        }

        [Parameter(10, "短期均线")]
        public int Short{ get; set; }

        [Parameter(20, "长期均线")]
        public int Long { get; set; }

        [Parameter(3.0, "ATR止损系数")]
        public double AtrCoefficent { get; set; }

        private IEvaluationContext _context;
        private double _maxRiskOfTotalCapital = 0.02;
        private int _maxVolumeUnitForSingleObject = 100;
        private double _initialCapital;

        private Dictionary<ITradingObject, RuntimeMetrics> _metrics = new Dictionary<ITradingObject, RuntimeMetrics>();
        private Dictionary<string, double> _stopLoss = new Dictionary<string, double>();

        private List<Instruction> _instructions;
        private DateTime _period;
        private double _capitalInCurrentPeriod;

        public string Name
        {
            get { return "移动均线策略"; }
        }

        public string StrategyDescription
        {
            get { return "本策略使用两条均线，当短期均线向上交叉长期均线时买入，当短期均线向下交叉长期均线时卖出"; }
        }

        public bool SupportParallelization
        {
            get { return true; }
        }

        public IEnumerable<ParameterAttribute> GetParameterDefinitions()
        {
            return ParameterHelper.GetParameterAttributes(this);
        }

        public void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (parameterValues == null)
            {
                throw new ArgumentNullException("parameterValues");
            }

            ParameterHelper.SetParameterValues(this, parameterValues);

            if (Short >= Long)
            {
                throw new ArgumentException("short parameter is not smaller than long parameter");
            }

            _context = context;
            _initialCapital = context.GetCurrentCapital();

            _context.Log(string.Format("Short: {0}, Long: {1}", Short, Long));
        }

        public void WarmUp(ITradingObject tradingObject, Bar bar)
        {
            var runtimeMetrics = GetOrCreateRuntimeMetrics(tradingObject);

            lock (runtimeMetrics)
            {
                runtimeMetrics.Update(bar);
            }
        }

        private RuntimeMetrics GetOrCreateRuntimeMetrics(ITradingObject tradingObject)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException("tradingObject");
            }

            lock (_metrics)
            {
                if (!_metrics.ContainsKey(tradingObject))
                {
                    if (!_metrics.ContainsKey(tradingObject))
                    {
                        _metrics.Add(tradingObject, new RuntimeMetrics(Short, Long));
                    }
                }
                return _metrics[tradingObject];
            }
        }

        public void Finish()
        {
            _metrics.Clear();
        }

        public void StartPeriod(DateTime time)
        {
            _instructions = new List<Instruction>();
            _period = time;
            _capitalInCurrentPeriod = _context.GetCurrentCapital();
        }

        public void NotifyTransactionStatus(Transaction transaction)
        {
            _context.Log(transaction.ToString());

            if (transaction.Succeeded)
            {
                if (transaction.Action == TradingAction.OpenLong)
                {
                    if (_stopLoss.ContainsKey(transaction.Code))
                    {
                        double lossPrice = transaction.Price - _stopLoss[transaction.Code];
                        _stopLoss[transaction.Code] = lossPrice;
                    }
                }
                else if (transaction.Action == TradingAction.CloseLong)
                {
                    _stopLoss.Remove(transaction.Code);
                }
            }
            else
            {
                if (transaction.Action == TradingAction.OpenLong)
                {
                    if (_stopLoss.ContainsKey(transaction.Code))
                    {
                        _stopLoss.Remove(transaction.Code);
                    }
               }
            }
        }

        public void Evaluate(ITradingObject tradingObject, Bar bar)
        {
            if (bar.Invalid())
            {
                return;
            }

            var runtimeMetrics = GetOrCreateRuntimeMetrics(tradingObject);

            double previousShortMA;
            double previousLongMA;
            double currentShortMA;
            double currentLongMA;
            double atr;

            lock (runtimeMetrics)
            {
                previousShortMA = runtimeMetrics.ShortMA;
                previousLongMA = runtimeMetrics.LongMA;

                runtimeMetrics.Update(bar);

                currentShortMA = runtimeMetrics.ShortMA;
                currentLongMA = runtimeMetrics.LongMA;
                atr = runtimeMetrics.Atr;
            }

            bool shouldStopLoss = false;
            double loss = 0.0;

            lock (_stopLoss)
            {
                if (_stopLoss.TryGetValue(tradingObject.Code, out loss))
                {
                    if (loss > bar.ClosePrice)
                    {
                        shouldStopLoss = true;
                    }
                }
            }

            if (shouldStopLoss)
            {
                TryToSell(tradingObject, string.Format("stop loss at {0:0.00}", loss));
                return;
            }

            if (previousShortMA > previousLongMA && currentShortMA < currentLongMA)
            {
                string comments =
                    string.Format(
                        "prevShort:{0:0.00}; prevLong:{1:0.00}; curShort:{2:0.00}; curLong:{3:0.00}",
                        previousShortMA,
                        previousLongMA,
                        currentShortMA,
                        currentLongMA);

                TryToSell(tradingObject, comments);
            }
            else if (previousShortMA < previousLongMA && currentShortMA > currentLongMA)
            {
                // buy
                Instruction buyInstruction = null;
                double riskPerShare = 0.0;

                lock (_context)
                {
                    if (!_context.ExistsEquity(tradingObject.Code))
                    {
                        riskPerShare = atr * AtrCoefficent;
                        double riskPerUnit = tradingObject.VolumePerBuyingUnit * riskPerShare;
                    
                        int unitCount = (int)(_initialCapital * _maxRiskOfTotalCapital / riskPerUnit);
                        unitCount = Math.Min(unitCount, _maxVolumeUnitForSingleObject);

                        int volume = unitCount * tradingObject.VolumePerBuyingUnit;
                        double cost = volume * bar.ClosePrice;

                        if (cost < _capitalInCurrentPeriod)
                        {
                            long id = _context.GetUniqueInstructionId();
                            buyInstruction = 
                                new Instruction()
                                {
                                    Action = TradingAction.OpenLong,
                                    ID = id,
                                    Object = tradingObject,
                                    SubmissionTime = _period,
                                    Volume = volume,
                                    Comments = string.Format(
                                        "prevShort:{0:0.00}; prevLong:{1:0.00}; curShort:{2:0.00}; curLong:{3:0.00}",
                                        previousShortMA,
                                        previousLongMA,
                                        currentShortMA,
                                        currentLongMA)
                                };

                            _instructions.Add(buyInstruction);
                        }
                    }
                }

                if (buyInstruction != null)
                {
                    lock (_stopLoss)
                    {
                        _stopLoss.Add(tradingObject.Code, riskPerShare);
                    }
                }
            }
        }

        private void TryToSell(ITradingObject tradingObject, string comments)
        {
            // sell
            lock (_context)
            {
                if (_context.ExistsEquity(tradingObject.Code))
                {
                    int volume = _context.GetEquityDetails(tradingObject.Code).Sum(e => e.Volume);
                    long id = _context.GetUniqueInstructionId();
                    _instructions.Add(
                        new Instruction()
                        {
                            Action = TradingAction.CloseLong,
                            ID = id,
                            Object = tradingObject,
                            SubmissionTime = _period,
                            Volume = volume,
                            Comments = comments
                        });
                }
            }
        }

        public IEnumerable<Instruction> GetInstructions()
        {
            return _instructions;
        }

        public void EndPeriod()
        {
            _instructions = null;
        }
    }
}
