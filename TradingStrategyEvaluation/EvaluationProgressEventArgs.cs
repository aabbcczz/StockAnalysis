using System;

namespace StockAnalysis.TradingStrategy
{
    public sealed class EvaluationProgressEventArgs : EventArgs
    {
        public DateTime EvaluationPeriod { get; private set; }
        public double EvaluationPercentage { get; private set; }

        public EvaluationProgressEventArgs(DateTime period, double percentage)
        {
            EvaluationPeriod = period;
            EvaluationPercentage = percentage;
        }
    }
}
