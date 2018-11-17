using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public interface ITimeSeriesData
    {
        DateTime Time { get; }
    }
}
