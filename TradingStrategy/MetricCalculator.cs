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
        private Transaction[] _orderedHistory = null;
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
            _orderedHistory = history.History
                .OrderBy(t => t, new Transaction.DefaultComparer())
                .ToArray();
            _periods = _dataProvider.GetAllPeriods().ToArray();
        }

        public IEnumerable<TradeMetric> Calculate()
        {
            List<TradeMetric> metrics = new List<TradeMetric>();

            TradeMetric metric = GetTradeMetric(TradeMetric.CodeForAll, TradeMetric.NameForAll, 0.0, 0.0);
            if (metric == null)
            {
                return metrics;
            }
            else
            {
                metrics.Add(metric);
            }

            var codes = _orderedHistory
                .Select(t => t.Code)
                .GroupBy(c => c)
                .Select(g => g.Key);

            Parallel.ForEach(
                codes,
                (string code) =>
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
                        lock (metrics)
                        {
                            metrics.Add(metric);
                        }
                    }
                });

            return metrics;   
        }

        private double EstimateUsedCapital(Transaction[] transactions)
        {
            if (transactions == null)
            {
                throw new ArgumentNullException("transactions");
            }

            if (transactions.Length == 0)
            {
                return 0.0;
            }

            if (transactions[0].Action != TradingAction.OpenLong)
            {
                throw new ArgumentException("First transaction is not openning long");
            }

            double usedCapital = 0.0;
            double currentCapital = 0.0;

            for (int i = 0; i < transactions.Length; ++i)
            {
                Transaction transaction = transactions[i];
                if (!transaction.Succeeded)
                {
                    continue;
                }

                if (transaction.Action == TradingAction.OpenLong)
                {
                    double capitalForThisTransaction = 
                        transaction.Price * transaction.Volume + transaction.Commission;

                    if (capitalForThisTransaction > currentCapital)
                    {
                        usedCapital += capitalForThisTransaction - currentCapital;
                        currentCapital = 0.0;
                    }
                    else
                    {
                        currentCapital -= capitalForThisTransaction;
                    }
                }
                else if (transaction.Action == TradingAction.CloseLong)
                {
                    double capitalForThisTransaction = 
                        transaction.Price * transaction.Volume - transaction.Commission;

                    currentCapital += capitalForThisTransaction;
                }
                else
                {
                    throw new InvalidProgramException("Logic error");
                }
            }

            return usedCapital + 1.0; // add 1.0 to avoid accumulated precision loss.
        }

        private TradeMetric GetTradeMetric(string code, string name, double startPrice, double endPrice)
        {
            Transaction[] transactions = 
                code == TradeMetric.CodeForAll 
                ? _orderedHistory
                : _orderedHistory.Where(t => t.Code == code).ToArray();

            double usedCapital = EstimateUsedCapital(transactions);

            EquityManager manager = new EquityManager(usedCapital);

            int transactionIndex = 0;
            double currentEquity = usedCapital; 

            List<CompletedTransaction> completedTransactions = new List<CompletedTransaction>(transactions.Length / 2 + 1);
            List<EquityPoint> equityPoints = new List<EquityPoint>(_periods.Length);

            for (int i = 0; i < _periods.Length; ++i)
            {
                DateTime period = _periods[i];

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
                    }
                    else
                    {
                        break;
                    }
                }

                if (manager.PositionCount > 0)
                {
                    // if any transaction is executed, update the total equity.
                    currentEquity = manager.GetTotalEquity(_dataProvider, period, EquityEvaluationMethod.TotalEquity);
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
                equityPoints.OrderBy(t => t.Time).ToArray(),
                completedTransactions.OrderBy(ct => ct, new CompletedTransaction.DefaultComparer()).ToArray(),
                transactions);
        }
    }
}
