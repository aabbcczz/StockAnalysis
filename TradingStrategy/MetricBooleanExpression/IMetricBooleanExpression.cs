using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.TradingStrategy.MetricBooleanExpression
{
    public interface IMetricBooleanExpression
    {
        void Initialize(IRuntimeMetricManager manager);

        bool IsTrue(ITradingObject tradingObject);

        string GetInstantializedExpression(ITradingObject tradingObject);
    }
}
