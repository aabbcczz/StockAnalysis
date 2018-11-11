using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy;
using MetricsDefinition.Metrics;

namespace TradingStrategyEvaluation
{
    public sealed class TradeMetricsCalculator
    {
        public const int ERatioAtrWindowSize = 20;

        public static readonly int[] ERatioWindowSizes = new int[]
        {
            10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120
        };

        private readonly Transaction[] _transactionHistory;
        private readonly CompletedTransaction[] _completedTransactionHistory;

        private readonly double _initialCapital;

        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        //private readonly StockNameTable _nameTable;
        private readonly ITradingDataProvider _dataProvider;
        private readonly DateTime[] _periods;
        private readonly TradingSettings _settings;

        public TradeMetricsCalculator(
            //StockNameTable nameTable,
            TradingTracker tracker, 
            ITradingDataProvider provider,
            TradingSettings settings)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException("history");
            }

            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            var startDate = provider.GetFirstNonWarmupDataPeriods().Min();

            var periods = provider.GetAllPeriodsOrdered().Where(d => d >= startDate).ToArray();
            
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
            _settings = settings;

            _startDate = startDate;
            _endDate = endDate;

            _initialCapital = tracker.InitialCapital;
            _transactionHistory = tracker.TransactionHistory.ToArray();

            _completedTransactionHistory = tracker.CompletedTransactionHistory.ToArray();

            _periods = periods;
        }

        public IEnumerable<TradeMetric> Calculate()
        {
            var metrics = new List<TradeMetric>();

            var overallMetric = GetTradeMetric(TradeMetric.SymbolForAll, TradeMetric.NameForAll, 0.0, 0.0);
            if (overallMetric == null)
            {
                return metrics;
            }

            metrics.Add(overallMetric);

            /* metrics for each symbol is not necessary now.
             * 
            var symbols = _orderedTransactionHistory
                .Select(t => t.Symbol)
                .GroupBy(c => c)
                .Select(g => g.Key);

            Parallel.ForEach(
                symbols,
                (string symbol) =>
                {
                    int index = _dataProvider.GetIndexOfTradingObject(symbol);

                    Bar[] bars = _dataProvider.GetAllBarsForTradingObject(index);

                    if (bars == null || bars.Length == 0)
                    {
                        throw new InvalidOperationException("logic error");
                    }

                    double startPrice = bars.First().ClosePrice;

                    double endPrice = bars.Last().ClosePrice;

                    string name = _nameTable.ContainsStock(symbol) ? _nameTable[symbol].Names[0] : string.Empty;

                    var metric = GetTradeMetric(symbol, name, startPrice, endPrice);

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

        private double GetCostOfTranscation(Transaction t)
        {
            return t.Action == TradingAction.OpenLong ? t.Price * t.Volume + t.Commission : 0.0;
        }

        private double GetReturnOfTransaction(Transaction t)
        {
            return t.Action == TradingAction.CloseLong ? t.Price * t.Volume - t.Commission : 0.0;
        }

        private double EstimateRequiredInitialCapital(Transaction[] transactions, double initialCapital)
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
                throw new ArgumentException("First transaction is not opening long");
            }

            // group succeeded transaction by date firstly
            var orderedTransactionGroups = transactions.Where(t => t.Succeeded).GroupBy(t => t.ExecutionTime).OrderBy(g => g.Key);

            if (orderedTransactionGroups.Count() == 0)
            {
                return 1.0;
            }

            // initial capital should be enough for all first day transcations (open)
            initialCapital = orderedTransactionGroups.First().Sum(t => GetCostOfTranscation(t));
            var maxUsedCapital = initialCapital;
            var requiredInitialCapital = initialCapital;
            var currentCapital = initialCapital;

            foreach (var group in orderedTransactionGroups)
            {
                double cost = group.Sum(t => GetCostOfTranscation(t));
                double gain = group.Sum(t => GetReturnOfTransaction(t));

                if (cost > currentCapital)
                {
                    var difference = cost - currentCapital;

                    requiredInitialCapital += difference ;

                    maxUsedCapital += difference;

                    currentCapital = 0.0;
                }
                else
                {
                    currentCapital -= cost;
                }

                currentCapital += gain;
            }

            return requiredInitialCapital + 1.0; // add 1.0 to avoid accumulated precision loss.
        }

        /// <summary>
        /// Calculate the normalized Maximum Favorable Excursion and Maximum Adversed Excursion for a given point
        /// </summary>
        /// <param name="bars"></param>
        /// <param name="barIndex"></param>
        /// <returns></returns>
        public static void CalculateNormalizedMfeAndMae(Bar[] bars, int barIndex, out double[] mfe, out double[] mae)
        {
            AverageTrueRange atr = new AverageTrueRange(ERatioAtrWindowSize);
            int startIndex = Math.Max(0, barIndex - ERatioAtrWindowSize);

            for (int i = startIndex; i < barIndex; ++i)
            {
                atr.Update(bars[i]);
            }

            double initialPrice = bars[barIndex].ClosePrice;

            mfe = new double[ERatioWindowSizes.Length];
            mae = new double[ERatioWindowSizes.Length];

            for (int i = 0; i < ERatioWindowSizes.Length; ++i)
            {
                var windowSize = ERatioWindowSizes[i];

                var highestPrice = Enumerable
                    .Range(barIndex, Math.Min(bars.Length - barIndex, windowSize))
                    .Max(index => bars[index].ClosePrice);

                var lowestPrice = Enumerable
                    .Range(barIndex, Math.Min(bars.Length - barIndex, windowSize))
                    .Min(index => bars[index].ClosePrice);

                mfe[i] = (highestPrice - initialPrice) / atr.Value;
                mae[i] = (initialPrice - lowestPrice) / atr.Value;
            }
        }

        private double[] CalculateERatio(IEnumerable<Transaction> orderedTransactions)
        {
            var symbols = orderedTransactions
                .Select(t => t.Symbol)
                .GroupBy(c => c)
                .Select(g => g.Key);

            List<double[]> mfes = new List<double[]>();
            List<double[]> maes = new List<double[]>();

            foreach (var symbol in symbols)
            {
                var bars = _dataProvider.GetAllBarsForTradingObject(_dataProvider.GetIndexOfTradingObject(symbol))
                    .ToArray();

                var subsetTransactions = orderedTransactions.Where(t => t.Symbol == symbol);

                long volume = 0;
                int barIndex = 0;
                foreach (var transaction in subsetTransactions)
                {
                    if (transaction.Action == TradingAction.OpenLong && volume == 0)
                    {
                        // this transaction is for entering market
                        // find the location of bar in bars for the transaction
                        while (barIndex < bars.Length)
                        {
                            if (bars[barIndex].Time == transaction.ExecutionTime)
                            {
                                break;
                            }

                            ++barIndex;
                        }

                        if (barIndex >= bars.Length)
                        {
                            // impossible
                            throw new InvalidOperationException("Logic error");
                        }

                        // calculate MFE and MAE
                        double[] mfe;
                        double[] mae;

                        CalculateNormalizedMfeAndMae(bars, barIndex, out mfe, out mae);

                        mfes.Add(mfe);
                        maes.Add(mae);
                    }

                    volume += transaction.Action == TradingAction.OpenLong ? transaction.Volume : -transaction.Volume;
                }
            }

            // calculate averge mfe and mae
            var avgMfe = Enumerable
                .Range(0, ERatioWindowSizes.Length)
                .Select(i => mfes.Select(v => v[i]).Average())
                .ToArray();

            var avgMae = Enumerable
                .Range(0, ERatioWindowSizes.Length)
                .Select(i => maes.Select(v => v[i]).Average())
                .ToArray();

            return Enumerable
                .Range(0, ERatioWindowSizes.Length)
                .Select(i => avgMfe[i] / avgMae[i])
                .ToArray();
        }

        private TradeMetric GetTradeMetric(string symbol, string name, double startPrice, double endPrice)
        {
            var completedTransactions =
                symbol == TradeMetric.SymbolForAll
                ? _completedTransactionHistory
                : _completedTransactionHistory.Where(ct => ct.Symbol == symbol).ToArray();

            if (completedTransactions.Length == 0)
            {
                return null;
            }

            var transactions = 
                symbol == TradeMetric.SymbolForAll 
                ? _transactionHistory
                : _transactionHistory.Where(t => t.Symbol == symbol).ToArray();

            var requiredInitialCapital = EstimateRequiredInitialCapital(transactions, _initialCapital);
            var initialCapital = Math.Max(_initialCapital, requiredInitialCapital);

            var manager = new EquityManager(new SimpleCapitalManager(initialCapital), _settings.PositionFrozenDays);

            var transactionIndex = 0;
            var currentEquity = initialCapital; 

            var equityPoints = new List<EquityPoint>(_periods.Length);

            // Calculate E-Ratio
            var eRatios = CalculateERatio(transactions);

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
                                    out error,
                                    true))
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
                symbol,
                name,
                _startDate,
                _endDate,
                startPrice,
                endPrice,
                equityPoints.OrderBy(t => t.Time).ToArray(),
                completedTransactions,
                transactions,
                eRatios);

            return metric;
        }
    }
}
