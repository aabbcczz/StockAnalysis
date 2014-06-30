using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace MetricsDefinition
{
    abstract class MetricExpression
    {
        public abstract string[] GetFieldNames();

        public abstract double[][] Evaluate(double[][] data);
    }
}
