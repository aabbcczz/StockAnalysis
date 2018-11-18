using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategyEvaluation
{
    public interface ICapitalManager
    {
        double InitialCapital { get; }

        double CurrentCapital { get; }

        bool AllocateCapital(double requiredCapital, bool forFirstPosition, bool allowNegativeCapital);

        void FreeCapital(double returnedCapital, bool forFirstPosition);
    }
}
