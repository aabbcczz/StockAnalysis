using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class EvaluationProgressEventArgs : EventArgs
    {
        public DateTime EvaluationPeriod { get; private set; }
        public double EvaluationPercentage { get; private set; }

        public EvaluationProgressEventArgs(DateTime period, double percentage)
            : base()
        {
            EvaluationPeriod = period;
            EvaluationPercentage = percentage;
        }
    }
}
