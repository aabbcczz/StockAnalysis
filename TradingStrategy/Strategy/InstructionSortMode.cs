using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public enum InstructionSortMode
    {
        NoSorting = 0,
        Randomize,
        SortByInstructionIdAscending,
        SortByInstructionIdDescending,
        SortByCodeAscending,
        SortByCodeDescending,
        SortByVolumeAscending,
        SortByVolumeDescending,
    }
}
