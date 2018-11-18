﻿
namespace StockAnalysis.TradingStrategy.Base
{
    public enum InstructionSortMode
    {
        NoSorting = 0,
        Randomize = 1,
        SortByInstructionIdAscending = 2,
        SortByInstructionIdDescending = 3,
        SortBySymbolAscending = 4,
        SortBySymbolDescending = 5,
        SortByVolumeAscending = 6,
        SortByVolumeDescending = 7,
        SortByMetricAscending = 8,
        SortByMetricDescending = 9
    }
}
