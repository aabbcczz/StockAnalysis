namespace DataAccess
{
    using StockAnalysis.Share;
    using System.Collections.Generic;
    using System;

    public interface IDataAccessor<T> where T : ITimeSeriesData
    {
        IEnumerable<T> ReadData(DataDescription description, SecuritySymbol symbol, DateTime startTimeInclusive, DateTime endTimeExclusive);

        void WriteData(IEnumerable<T> data, DataDescription description, SecuritySymbol symbol, DateTime startTimeInclusive, DateTime endTimeExclusive);
    }
}
