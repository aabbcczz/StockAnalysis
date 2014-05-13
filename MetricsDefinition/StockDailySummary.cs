using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    public class StockDailySummary
    {
        public DateTime Date { get; set; } // transaction date
        public double OpenMarketPrice { get; set; } // price when openning market
        public double CloseMarketPrice { get; set; } // price when closing market
        public double HighestPrice { get; set; } // highest price in the whole day
        public double LowestPrice { get; set; } // lowest price in the whole day
        public double AmountOfSharesInAllTransaction { get; set; } // total amount of shares in all transactions
        public double AmountOfMoneyInAllTransaction { get; set; } // total amount of money in all transaction
    }
}
