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
            ITradingDataProvider provider, 
            DateTime startDate, 
            DateTime endDate)
        {
            if (history == null)
            {
                throw new ArgumentNullException("history");
            }

            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            startDate = startDate.Date;
            endDate = endDate.Date;

            if (startDate >= endDate)
            {
                throw new ArgumentException("startDate must be earlier than endDate");
            }

            if (history.MinTransactionTime < startDate)
            {
                throw new ArgumentOutOfRangeException("startDate is not smaller than the minimum transaction time in trading history");
            }

            if (history.MaxTransactionTime >= endDate)
            {
                throw new ArgumentOutOfRangeException("endDate is not larger than the maximum transaction time in trading history");
            }

            _nameTable = nameTable;
            _dataProvider = provider;

            _startDate = startDate;
            _endDate = endDate;

            _initialCapital = history.InitialCapital;
            _orderedHistory = history.History.OrderBy(t => t, new Transaction.DefaultComparer());
            _periods = _dataProvider.GetAllPeriods().Where(p => p >= _startDate && p <= _endDate).ToArray();
        }

        public IEnumerable<TradeMetric> Calculate()
        {
            yield return GetTradeMetric(TradeMetric.CodeForAll, TradeMetric.NameForAll, 0.0, 0.0);

            var codes = _orderedHistory
                .Select(t => t.Code)
                .GroupBy(c => c)
                .Select(g => g.Key);

            foreach (var code in codes)
            {
                Bar bar;

                if (!_dataProvider.GetLastEffectiveBar(code, _periods.First(), out bar))
                {
                    throw new InvalidOperationException("logic error");
                }

                double startPrice = bar.ClosePrice;

                if (!_dataProvider.GetLastEffectiveBar(code, _periods.Last(), out bar))
                {
                    throw new InvalidOperationException("logic error");
                }

                double endPrice = bar.ClosePrice;

                string name = _nameTable.ContainsStock(code) ? _nameTable[code].Names[0] : string.Empty;

                yield return GetTradeMetric(code, name, startPrice, endPrice);
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
                    string error;
                    if (transactions[transactionIndex].ExecutionTime < period)
                    {
                        if (!manager.ExecuteTransaction(
                                transactions[transactionIndex], 
                                out completedTransaction, 
                                out error))
                        {
                            throw new InvalidOperationException("Replay transaction failed: " + error);
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
