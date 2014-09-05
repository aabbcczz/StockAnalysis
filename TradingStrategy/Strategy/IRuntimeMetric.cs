using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public interface IRuntimeMetric
    {
        void Update(StockAnalysis.Share.Bar bar);
    }
}
