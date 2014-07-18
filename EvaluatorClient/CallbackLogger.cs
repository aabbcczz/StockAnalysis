using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;

namespace EvaluatorClient
{
    class CallbackLogger : ILogger
    {
        public delegate void LogHandler(string log);

        public LogHandler OnLog { get; set; }

        public void Log(string log)
        {
            if (OnLog != null)
            {
                OnLog(log);
            }
        }
    }
}
