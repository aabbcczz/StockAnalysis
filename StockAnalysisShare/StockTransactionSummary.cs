using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public class StockTransactionSummary
    {
        public DateTime Time { get; set; } // transaction time
        public double OpenPrice { get; set; } // price when openning market
        public double ClosePrice { get; set; } // price when closing market
        public double HighestPrice { get; set; } // highest price in the whole day
        public double LowestPrice { get; set; } // lowest price in the whole day
        public double Volume { get; set; } // total amount of volume in all transactions
        public double Amount { get; set; } // total amount of money in all transaction
    }
}
