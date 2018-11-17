namespace DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using StockAnalysis.Share;

    internal class FileStorageAccessor<T> : IDataAccessor<T>
        where T : ITimeSeriesData
    {
        public IEnumerable<T> ReadData(DataDescription description, SecuritySymbol symbol, DateTime startTimeInclusive, DateTime endTimeExclusive)
        {
            throw new NotImplementedException();
        }

        public void WriteData(IEnumerable<T> data, DataDescription description, SecuritySymbol symbol, DateTime startTimeInclusive, DateTime endTimeExclusive)
        {
            throw new NotImplementedException();
        }
    }
}
