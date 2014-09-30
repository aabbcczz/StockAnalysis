using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;

namespace TradingStrategyEvaluation
{
    public sealed class MemoryLogger : ILogger
    {
        private List<string> _logs = new List<string>();

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
