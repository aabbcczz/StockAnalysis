namespace StockAnalysis.TradingStrategy
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class DeprecatedStrategyAttribute : Attribute
    {
        public DeprecatedStrategyAttribute()
        {
        }
    }
}
