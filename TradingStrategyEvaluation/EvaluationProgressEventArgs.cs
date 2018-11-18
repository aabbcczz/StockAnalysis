namespace StockAnalysis.TradingStrategy
{
    using System;

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
