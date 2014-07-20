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

        private IEvaluationContext _context;
        private int _short;
        private int _long;
        private double _maxRiskOfTotalCapital = 0.02;
        private double _initialCapital;

        private Dictionary<ITradingObject, RuntimeMetrics> _metrics = new Dictionary<ITradingObject, RuntimeMetrics>();

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

        public string ParameterDescription
        {
            get { return "本策略需要两个参数： <短期均线周期数> 和 <长期均线周期数>, 例如： 30, 60"; }
        }

        public void Initialize(IEvaluationContext context, string[] parameters)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            if (parameters.Length < 2)
            {
                throw new ArgumentException("Parameter string should include at least two fields: short and long");
            }

            _short = int.Parse(parameters[0]);
            _long = int.Parse(parameters[1]);

            if (_short >= _long)
            {
                throw new ArgumentException("short parameter is not smaller than long parameter");
            }

            _context = context;
            _initialCapital = context.GetCurrentCapital();
        }

        public void WarmUp(ITradingObject tradingObject, Bar bar)
        {
            var runtimeMetrics = GetOrCreateRuntimeMetrics(tradingObject);
            runtimeMetrics.Update(bar);
        }

        private RuntimeMetrics GetOrCreateRuntimeMetrics(ITradingObject tradingObject)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException("tradingObject");
            }

            if (!_metrics.ContainsKey(tradingObject))
            {
                _metrics.Add(tradingObject, new RuntimeMetrics(_short, _long));
            }

            return _metrics[tradingObject];
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
        }

        public void Evaluate(ITradingObject tradingObject, Bar bar)
        {
            if (bar.Invalid())
            {
                return;
            }

            var runtimeMetrics = GetOrCreateRuntimeMetrics(tradingObject);

            double previousShortMA = runtimeMetrics.ShortMA;
            double previousLongMA = runtimeMetrics.LongMA;

            runtimeMetrics.Update(bar);

            double currentShortMA = runtimeMetrics.ShortMA;
            double currentLongMA = runtimeMetrics.LongMA;
            double atr = runtimeMetrics.Atr;

            if (previousShortMA > previousLongMA && currentShortMA < currentLongMA)
            {
                // sell
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
                            Volume = volume
                        });

                    _context.Log(
                        string.Format(
                            "{0} {1:yyyy-MM-dd HH:mm:ss}: try to sell {2} vol {3} [ps:{4:0.00}, pl:{5:0.00}, cs:{6:0.00}, cl:{7:0.00}]",
                            id,
                            _period,
                            tradingObject.Code,
                            volume,
                            previousShortMA,
                            previousLongMA,
                            currentShortMA,
                            currentLongMA));
                }
            }
            else if (previousShortMA < previousLongMA && currentShortMA > currentLongMA)
            {
                if (!_context.ExistsEquity(tradingObject.Code))
                {
                    // buy
                    double risk = tradingObject.VolumePerBuyingUnit * atr;
                    int unit = (int)(_initialCapital * _maxRiskOfTotalCapital / risk);
                    int volume = unit * tradingObject.VolumePerBuyingUnit;
                    double cost = volume * bar.ClosePrice;

                    if (cost < _capitalInCurrentPeriod)
                    {
                        long id = _context.GetUniqueInstructionId();
                        _instructions.Add(
                            new Instruction()
                            {
                                Action = TradingAction.OpenLong,
                                ID = id,
                                Object = tradingObject,
                                SubmissionTime = _period,
                                Volume = volume
                            });

                        _context.Log(
                            string.Format(
                                "{0} {1:yyyy-mm-dd HH:mm:ss}: try to buy {2} vol {3} [ps:{4:0.00}, pl:{5:0.00}, cs:{6:0.00}, cl:{7:0.00}]",
                                id,
                                _period,
                                tradingObject.Code,
                                volume,
                                previousShortMA,
                                previousLongMA,
                                currentShortMA,
                                currentLongMA));
                    }
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
