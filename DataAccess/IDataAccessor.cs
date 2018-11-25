namespace StockAnalysis.DataAccess
{
    using Common.Data;
    using Common.SymbolName;

    using System.Collections.Generic;
    using System;

    public interface IDataAccessor
    {
        IEnumerable<T> ReadData<T>(DataDescription description, SecuritySymbol symbol, DateTime startTimeInclusive, DateTime endTimeExclusive)
            where T : struct, ITimeSeriesData;

        void WriteData<T>(IEnumerable<T> data, DataDescription description, SecuritySymbol symbol)
            where T : struct, ITimeSeriesData;
    }
}
