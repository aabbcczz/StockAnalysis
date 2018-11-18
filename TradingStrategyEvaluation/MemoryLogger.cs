using System.Collections.Generic;
using StockAnalysis.TradingStrategy;

namespace StockAnalysis.TradingStrategy.Evaluation
{
    public sealed class MemoryLogger : ILogger
    {
        private readonly List<string> _logs = new List<string>();

        public IEnumerable<string> Logs
        {
            get { return _logs; }
        }

        public void Log(string log)
        {
            _logs.Add(log);
        }
    }
}
