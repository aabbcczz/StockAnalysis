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
        private class DoubleMovingAverage
        {
            public MovingAverage Short { get; private set; }
            public MovingAverage Long {get; private set; }

            public DoubleMovingAverage(int shortPeriod, int longPeriod)
            {
                Short = new MovingAverage(shortPeriod);
                Long = new MovingAverage(longPeriod);
            }
        }

        private IEvaluationContext _context;
        private int _short;
        private int _long;

        private Dictionary<ITradingObject, DoubleMovingAverage> _metrics = new Dictionary<ITradingObject, DoubleMovingAverage>();

        public string Name
        {
            get { return "移动均线策略"; }
        }

        public string StrategyDescription
        {
            get { return "本策略使用两条均线，当长期均线向上交叉短期均线时买入，当短期均线向下交叉长期均线时卖出"; }
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
        }

        public void WarmUp(ITradingObject tradingObject, Bar bar)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException("tradingObject");
            }

            if (!_metrics.ContainsKey(tradingObject))
            {
                _metrics.Add(tradingObject, new DoubleMovingAverage(_short, _long));
            }

            var dma = _metrics[tradingObject];
            dma.Short.Update(bar.ClosePrice);
            dma.Long.Update(bar.ClosePrice);
        }

        public void Finish()
        {
            _metrics.Clear();
        }

        public void StartPeriod(DateTime time)
        {
        }

        public void NotifyTransactionStatus(Transaction transaction)
        {
        }

        public void Evaluate(ITradingObject tradingObject, Bar bar)
        {
        }

        public IEnumerable<Instruction> GetInstructions()
        {
            return new List<Instruction>();
        }

        public void EndPeriod()
        {
        }
    }
}
