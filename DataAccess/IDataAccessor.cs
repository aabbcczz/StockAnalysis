namespace DataAccess
{
    using StockAnalysis.Common.Data;
    using StockAnalysis.Common.SymbolName;

    using System.Collections.Generic;
    using System;

    public interface IDataAccessor<T> where T : ITimeSeriesData
    {
        IEnumerable<T> ReadData(DataDescription description, SecuritySymbol symbol, DateTime startTimeInclusive, DateTime endTimeExclusive);

        void WriteData(IEnumerable<T> data, DataDescription description, SecuritySymbol symbol, DateTime startTimeInclusive, DateTime endTimeExclusive);
    }
}
