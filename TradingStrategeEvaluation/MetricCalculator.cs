using System;
using System.Collections.Generic;
using System.Linq;
//using StockAnalysis.Share;
using TradingStrategy;

namespace TradingStrategyEvaluation
{
    public sealed class MetricCalculator
    {
        private readonly Transaction[] _orderedTransactionHistory;
        private readonly CompletedTransaction[] _orderedCompletedTransactionHistory;

        private readonly double _initialCapital;

        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        //private readonly StockNameTable _nameTable;
        private readonly ITradingDataProvider _dataProvider;
        private readonly DateTime[] _periods;

        public MetricCalculator(
            //StockNameTable nameTable,
            TradingTracker tracker, 
            ITradingDataProvider provider)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException("history");
            }

            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            var periods = provider.GetAllPeriodsOrdered();

            var startDate = periods.First().Date;
            var endDate = periods.Last();
            if (endDate.Date < endDate)
            {
                endDate.AddDays(1);
            }

            if (tracker.MinTransactionTime < startDate)
            {
                throw new ArgumentOutOfRangeException("the minimum transaction time in trading history is smaller than the start date of provider's data");
            }

            if (tracker.MaxTransactionTime > endDate)
            {
                throw new ArgumentOutOfRangeException("the maximum transaction time in trading history is larger than the end date of provider's data");
            }

            //_nameTable = nameTable;
            _dataProvider = provider;

            _startDate = startDate;
            _endDate = endDate;

            _initialCapital = tracker.InitialCapital;
            _orderedTransactionHistory = tracker.TransactionHistory
                .OrderBy(t => t, new Transaction.DefaultComparer())
                .ToArray();

            _orderedCompletedTransactionHistory = tracker.CompletedTransactionHistory
                .OrderBy(ct => ct, new CompletedTransaction.DefaultComparer())
                .ToArray();

            _periods = _dataProvider.GetAllPeriodsOrdered();
        }

        public IEnumerable<TradeMetric> Calculate()
        {
            var metrics = new List<TradeMetric>();

            var overallMetric = GetTradeMetric(TradeMetric.CodeForAll, TradeMetric.NameForAll, 0.0, 0.0);
            if (overallMetric == null)
            {
                return metrics;
            }

            metrics.Add(overallMetric);

            /* metrics for each code is not necessary now.
             * 
            var codes = _orderedTransactionHistory
                .Select(t => t.Code)
                .GroupBy(c => c)
                .Select(g => g.Key);

            Parallel.ForEach(
                codes,
                (string code) =>
                {
                    int index = _dataProvider.GetIndexOfTradingObject(code);

                    Bar[] bars = _dataProvider.GetAllBarsForTradingObject(index);

                    if (bars == null || bars.Length == 0)
                    {
                        throw new InvalidOperationException("logic error");
                    }

                    double startPrice = bars.First().ClosePrice;

                    double endPrice = bars.Last().ClosePrice;

                    string name = _nameTable.ContainsStock(code) ? _nameTable[code].Names[0] : string.Empty;

                    var metric = GetTradeMetric(code, name, startPrice, endPrice);

                    if (metric != null)
                    {
                        lock (metrics)
                        {
                            metrics.Add(metric);
                        }
                    }
                });
             */

            return metrics;   
        }

/*
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

            var usedCapital = 0.0;
            var currentCapital = 0.0;

            for (var i = 0; i < transactions.Length; ++i)
            {
                var transaction = transactions[i];
                if (!transaction.Succeeded)
                {
                    continue;
                }

                if (transaction.Action == TradingAction.OpenLong)
                {
                    var capitalForThisTransaction = 
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
                    var capitalForThisTransaction = 
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
*/

        private TradeMetric GetTradeMetric(string code, string name, double startPrice, double endPrice)
        {
            var completedTransactions =
                code == TradeMetric.CodeForAll
                ? _orderedCompletedTransactionHistory
                : _orderedCompletedTransactionHistory.Where(ct => ct.Code == code).ToArray();

            if (completedTransactions.Length == 0)
            {
                return null;
            }

            var transactions = 
                code == TradeMetric.CodeForAll 
                ? _orderedTransactionHistory
                : _orderedTransactionHistory.Where(t => t.Code == code).ToArray();

            //double usedCapital = EstimateUsedCapital(transactions);

            var manager = new EquityManager(_initialCapital);

            var transactionIndex = 0;
            var currentEquity = _initialCapital; 

            var equityPoints = new List<EquityPoint>(_periods.Length);

            foreach (var period in _periods)
            {
                while (transactionIndex < transactions.Length)
                {
                    var transaction = transactions[transactionIndex];

                    if (transaction.ExecutionTime <= period)
                    {
                        if (transaction.Succeeded)
                        {
                            // save transaction selling type
                            var sellingType = transaction.SellingType;

                            // always use ByVolume selling type to simulate transactions.
                            transaction.SellingType = SellingType.ByVolume;

                            CompletedTransaction completedTransaction;
                            string error;
                            if (!manager.ExecuteTransaction(
                                transaction,
                                true,
                                out completedTransaction,
                                out error))
                            {
                                throw new InvalidOperationException("Replay transaction failed: " + error);
                            }

                            // recover the transaction selling type
                            transaction.SellingType = sellingType;
                        }

                        ++transactionIndex;
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

                equityPoints.Add(
                    new EquityPoint
                    { 
                        Capital = manager.CurrentCapital,
                        Equity = currentEquity, 
                        Time = period 
                    });
            }


            var metric = new TradeMetric();

            metric.Initialize(
                code,
                name,
                _startDate,
                _endDate,
                startPrice,
                endPrice,
                equityPoints.OrderBy(t => t.Time).ToArray(),
                completedTransactions,
                transactions);

            return metric;
        }
    }
}
