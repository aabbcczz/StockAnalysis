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

        bool AllocateCapitalForFirstPosition(double requiredCapital, bool allowNegativeCapital);

        bool AllocateCapitalForIncrementalPosition(double requiredCapital, bool allowNegativeCapital);

        void FreeCapitalForFirstPosition(double returnedCapital);

        void FreeCapitalForIncrementalPosition(double returnedCapital);
    }
}
