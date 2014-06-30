using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace MetricsDefinition
{
    public abstract class MetricExpression
    {
        public abstract string[] FieldNames { get; }

        public virtual double SingleOutputUpdate(Bar data)
        {
            throw new NotImplementedException();
        }

        public virtual double[] MultipleOutputUpdate(Bar data)
        {
            throw new NotImplementedException();
        }

        public virtual double SingleOutputUpdate(double data)
        {
            throw new NotImplementedException();
        }

        public virtual double[] MultipleOutputUpdate(double data)
        {
            throw new NotImplementedException();
        }
    }
}
