using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using TradingStrategy;
using TradingStrategyEvaluation;
using CsvHelper;

namespace EvaluatorCmdClient
{
    public sealed class EvaluationResultContext : IDisposable
    {
        private const string LogFileName = "Log.txt";
        private const string ParameterValuesFileName = "ParameterValues.xml";
        private const string MetricsFileName = "Metrics.csv";
        private const string ClosePositionsFileName = "ClosedPositions.csv";
        private const string EquitiesFileName = "Equities.csv";
        private const string TransactionsFileName = "Transactions.csv";
        private const string CompletedTransactionsFileName = "CompletedTransactions.csv";

        public string RootDirectory { get; private set; }
        public int ContextId { get; private set; }

        private bool _disposed;

        public ILogger Logger { get; private set; }

        public EvaluationResultContext(int contextId, string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                throw new ArgumentNullException();
            }

            RootDirectory = rootDirectory;
            ContextId = contextId;

            var logFile = Path.Combine(rootDirectory, LogFileName);
            Logger = new FileLogger(logFile);
        }

        public void SaveResults(
            IDictionary<ParameterAttribute, object> parameterValues,
            IEnumerable<TradeMetric> metrics, 
            IEnumerable<Position> closePositions)
        {
            // save parameter values
            var searializedParameterValues = new SerializableParameterValues();
            searializedParameterValues.Initialize(parameterValues);

            using (var writer = new StreamWriter(
                Path.Combine(RootDirectory, ParameterValuesFileName), 
                false, 
                Encoding.UTF8))
            {
                var serializer = new XmlSerializer(searializedParameterValues.GetType());
                serializer.Serialize(writer, searializedParameterValues);
            }

            // save metrics
            var tradeMetrics = metrics as TradeMetric[] ?? metrics.ToArray();
            using (var writer = new StreamWriter(
                Path.Combine(RootDirectory, MetricsFileName),
                false,
                Encoding.UTF8))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.WriteRecords(tradeMetrics);
                }
            }

            // save closed positions
            using (var writer = new StreamWriter(
                Path.Combine(RootDirectory, ClosePositionsFileName),
                false,
                Encoding.UTF8))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.WriteRecords(closePositions);
                }
            }

            // get the overall metric
            var overallMetric = tradeMetrics.First(m => m.Code == TradeMetric.CodeForAll);
            
            // save equities
            using (var writer = new StreamWriter(
                Path.Combine(RootDirectory, EquitiesFileName),
                false,
                Encoding.UTF8))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.WriteRecords(overallMetric.OrderedEquitySequence);
                }
            }

            // save transactions
            using (var writer = new StreamWriter(
                Path.Combine(RootDirectory, TransactionsFileName),
                false,
                Encoding.UTF8))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.WriteRecords(overallMetric.OrderedTransactionSequence);
                }
            }

            // save closed transactions
            using (var writer = new StreamWriter(
                Path.Combine(RootDirectory, CompletedTransactionsFileName),
                false,
                Encoding.UTF8))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.WriteRecords(overallMetric.OrderedCompletedTransactionSequence);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(typeof(EvaluationResultContext).FullName);
            }

            if (Logger != null)
            {
                ((FileLogger)Logger).Dispose();
                Logger = null;
            }

            _disposed = true;
        }
    }
}
