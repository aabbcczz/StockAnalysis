using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    abstract class MetricUnaryOperator : MetricExpression
    {
        protected MetricExpression Operand { get; set; }

        protected MetricUnaryOperator(MetricExpression operand)
        {
            Operand = operand;
        }
    }
}
