namespace StockAnalysis.TradingStrategy.Evaluation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Strategy;

    public sealed class BlockTradingDetailSummarizer
    {
        public sealed class BlockTradingDetail
        {
            public string Symbol { get; set; }
            public DateTime Time { get; set; }
            public string Block { get; set; }
            public double UpRateFromLowest { get; set; }
            public double[] Mfe { get; set; }
            public double[] Mae { get; set; }
        }

        private readonly Transaction[] _transactionHistory;
        private readonly ITradingDataProvider _dataProvider;
        private readonly DateTime[] _periods;

        public BlockTradingDetailSummarizer(TradingTracker tracker, ITradingDataProvider provider)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException("history");
            }

            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            _dataProvider = provider;

            _transactionHistory = tracker.TransactionHistory.ToArray();

            _periods = _dataProvider.GetAllPeriodsOrdered();
        }

        public IEnumerable<BlockTradingDetail> Summarize()
        {
            var symbols = _transactionHistory
                .Select(t => t.Symbol)
                .GroupBy(c => c)
                .Select(g => g.Key);

            foreach (var symbol in symbols)
            {
                var bars = _dataProvider.GetAllBarsForTradingObject(_dataProvider.GetIndexOfTradingObject(symbol))
                    .ToArray();

                var subsetTransactions = _transactionHistory.Where(t => t.Symbol == symbol);

                int barIndex = 0;
                foreach (var transaction in subsetTransactions)
                {
                    if (transaction.RelatedObjects != null 
                        && transaction.RelatedObjects.Any(o => o is BlockPriceIndexFilterMarketEntering.BlockUpRatesFromLowestForSymbol))
                    {
                        // this transaction is for entering market and has block information.
                        BlockPriceIndexFilterMarketEntering.BlockUpRatesFromLowestForSymbol obj =
                            (BlockPriceIndexFilterMarketEntering.BlockUpRatesFromLowestForSymbol)
                            transaction.RelatedObjects.First(o => o is BlockPriceIndexFilterMarketEntering.BlockUpRatesFromLowestForSymbol);

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

                        TradeMetricsCalculator.CalculateNormalizedMfeAndMae(bars, barIndex, out mfe, out mae);

                        foreach (var kvp in obj.BlockUpRatesFromLowest)
                        {
                            yield return new BlockTradingDetail()
                            {
                                Symbol = symbol,
                                Time = transaction.ExecutionTime,
                                Block = kvp.Key,
                                UpRateFromLowest = kvp.Value,
                                Mfe = mfe,
                                Mae = mae,
                            };
                        }
                    }
                }
            }
        }
    }
}
