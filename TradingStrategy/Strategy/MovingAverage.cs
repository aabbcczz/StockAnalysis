using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverage : ITradingStrategy
    {
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
            get { return "参数格式为： <短期均线周期数>;<长期均线周期数>, 例如： 30;60"; }
        }

        public void Initialize(ITradingStrategyEvaluationContext context, string parameters)
        {
        }

        public void WarmUp(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
        }

        public void Finish()
        {
        }

        public void StartPeriod(DateTime time)
        {
        }

        public void NotifyTransactionStatus(Transaction transaction)
        {
        }

        public void Evaluate(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
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
