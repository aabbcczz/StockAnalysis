using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;
using TradingStrategy.Strategy;

namespace TradingStrategyEvaluation
{
    public sealed class BlockTradingDetailSummarizer
    {
        public sealed class BlockTradingDetail
        {
            public string Code { get; set; }
            public DateTime Time { get; set; }
            public string Block { get; set; }
            public double UpRateFromLowest { get; set; }
            public double[] Mfe { get; set; }
            public double[] Mae { get; set; }
        }

        private readonly Transaction[] _orderedTransactionHistory;
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

            _orderedTransactionHistory = tracker.TransactionHistory
                .OrderBy(t => t, new Transaction.DefaultComparer())
                .ToArray();

            _periods = _dataProvider.GetAllPeriodsOrdered();
        }

        public IEnumerable<BlockTradingDetail> Summarize()
        {
            var codes = _orderedTransactionHistory
                .Select(t => t.Code)
                .GroupBy(c => c)
                .Select(g => g.Key);

            foreach (var code in codes)
            {
                var bars = _dataProvider.GetAllBarsForTradingObject(_dataProvider.GetIndexOfTradingObject(code))
                    .ToArray();

                var subsetTransactions = _orderedTransactionHistory.Where(t => t.Code == code);

                int barIndex = 0;
                foreach (var transaction in subsetTransactions)
                {
                    if (transaction.RelatedObjects != null 
                        && transaction.RelatedObjects.Any(o => o is BlockPriceIndexFilterMarketEntering.BlockUpRatesFromLowestForCode))
                    {
                        // this transaction is for entering market and has block information.
                        BlockPriceIndexFilterMarketEntering.BlockUpRatesFromLowestForCode obj =
                            (BlockPriceIndexFilterMarketEntering.BlockUpRatesFromLowestForCode)
                            transaction.RelatedObjects.First(o => o is BlockPriceIndexFilterMarketEntering.BlockUpRatesFromLowestForCode);

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
                                Code = code,
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
