using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy
{
    public sealed class MetricCalculator
    {
        private IOrderedEnumerable<Transaction> _orderedHistory = null;
        private double _initialCapital = 0.0;

        private DateTime _startDate;
        private DateTime _endDate;

        private ITradingDataProvider _dataProvider;
        private DateTime[] _periods;
        private StockNameTable _nameTable;

        public MetricCalculator(
            StockNameTable nameTable,
            TradingHistory history, 
            ITradingDataProvider provider)
        {
            if (history == null)
            {
                throw new ArgumentNullException("history");
            }

            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            var periods = provider.GetAllPeriods();

            DateTime startDate = periods.First().Date;
            DateTime endDate = periods.Last();
            if (endDate.Date < endDate)
            {
                endDate.AddDays(1);
            }

            if (history.MinTransactionTime < startDate)
            {
                throw new ArgumentOutOfRangeException("the minimum transaction time in trading history is smaller than the start date of provider's data");
            }

            if (history.MaxTransactionTime > endDate)
            {
                throw new ArgumentOutOfRangeException("the maximum transaction time in trading history is larger than the end date of provider's data");
            }

            _nameTable = nameTable;
            _dataProvider = provider;

            _startDate = startDate;
            _endDate = endDate;

            _initialCapital = history.InitialCapital;
            _orderedHistory = history.History.OrderBy(t => t, new Transaction.DefaultComparer());
            _periods = _dataProvider.GetAllPeriods().ToArray();
        }

        public IEnumerable<TradeMetric> Calculate()
        {
            TradeMetric metric = GetTradeMetric(TradeMetric.CodeForAll, TradeMetric.NameForAll, 0.0, 0.0);
            if (metric == null)
            {
                yield break;
            }
            else
            {
                yield return metric;
            }

            var codes = _orderedHistory
                .Select(t => t.Code)
                .GroupBy(c => c)
                .Select(g => g.Key);

            foreach (var code in codes)
            {
                Bar[] bars = _dataProvider.GetAllBarsForTradingObject(code);

                if (bars == null || bars.Length == 0)
                {
                    throw new InvalidOperationException("logic error");
                }

                double startPrice = bars.First().ClosePrice;

                double endPrice = bars.Last().ClosePrice;

                string name = _nameTable.ContainsStock(code) ? _nameTable[code].Names[0] : string.Empty;

                metric = GetTradeMetric(code, name, startPrice, endPrice);
                
                if (metric != null)
                {
                    yield return metric;
                }
            }
        }


        private TradeMetric GetTradeMetric(string code, string name, double startPrice, double endPrice)
        {
            Transaction[] transactions = 
                code == TradeMetric.CodeForAll 
                ? _orderedHistory.ToArray()
                : _orderedHistory.Where(t => t.Code == code).ToArray();

            EquityManager manager = new EquityManager(_initialCapital);

            int transactionIndex = 0;
            double currentEquity = manager.InitialCapital;

            List<CompletedTransaction> completedTransactions = new List<CompletedTransaction>(transactions.Length / 2 + 1);
            List<EquityPoint> equityPoints = new List<EquityPoint>(_periods.Length);

            for (int i = 0; i < _periods.Length; ++i)
            {
                DateTime period = _periods[i];

                bool equityChanged = false;
                CompletedTransaction completedTransaction = null;

                while (transactionIndex < transactions.Length)
                {
                    Transaction transaction = transactions[transactionIndex];

                    string error;
                    if (transaction.ExecutionTime <= period)
                    {
                        if (transaction.Succeeded)
                        {
                            if (!manager.ExecuteTransaction(
                                    transaction,
                                    code != TradeMetric.CodeForAll,
                                    out completedTransaction,
                                    out error))
                            {
                                throw new InvalidOperationException("Replay transaction failed: " + error);
                            }
                        }

                        ++transactionIndex;
                        
                        if (completedTransaction != null)
                        {
                            completedTransactions.Add(completedTransaction);
                        }

                        equityChanged = true;
                    }
                    else
                    {
                        break;
                    }
                }

                if (equityChanged)
                {
                    // if any transaction is executed, update the total equity.
                    currentEquity = manager.GetTotalEquityMarketValue(_dataProvider, period);
                }

                equityPoints.Add(new EquityPoint() { Equity = currentEquity, Time = period });
            }

            if (completedTransactions.Count == 0)
            {
                return null;
            }

            return new TradeMetric(
                code,
                name,
                _startDate,
                _endDate,
                startPrice,
                endPrice,
                equityPoints.OrderBy(t => t.Time),
                completedTransactions.OrderBy(ct => ct, new CompletedTransaction.DefaultComparer()));
        }
    }
}
