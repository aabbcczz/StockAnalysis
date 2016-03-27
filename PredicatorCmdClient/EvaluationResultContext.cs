using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using TradingStrategy;
using TradingStrategyEvaluation;
using CsvHelper;
using StockAnalysis.Share;

namespace PredicatorCmdClient
{
    public sealed class PredicationContext : IDisposable
    {
        private const string LogFileName = "Log.txt";
        private const string ParameterValuesFileName = "ParameterValues.xml";
        private const string PositionFileName = "Positions.csv";
        private const string TransactionsFileName = "Transactions.csv";
        private const string AuxiliaryDataFileName = "Auxiliary.csv";

        public string RootDirectory { get; private set; }

        private bool _disposed;

        public ILogger Logger { get; private set; }

        public PredicationContext(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                throw new ArgumentNullException();
            }

            RootDirectory = rootDirectory;

            var logFile = Path.Combine(rootDirectory, LogFileName);
            Logger = new FileLogger(logFile);
        }

        public void SaveResults(
            ITradingDataProvider dataProvider,
            IDictionary<Tuple<int, ParameterAttribute>, object> parameterValues,
            IEnumerable<Position> activePositions,
            IEnumerable<Transaction> transactions,
            IEnumerable<AuxiliaryData> auxiliaryData)
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

            // save active positions
            using (var writer = new StreamWriter(
                Path.Combine(RootDirectory, PositionFileName),
                false,
                Encoding.UTF8))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.WriteHeader<Position>();

                    foreach (var position in activePositions)
                    {
                        csvWriter.WriteRecord(position);
                    }
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
                    csvWriter.WriteRecords(transactions);
                }
            }

            // save auxiliary data
            using (var writer = new StreamWriter(
                Path.Combine(RootDirectory, AuxiliaryDataFileName),
                false,
                Encoding.UTF8))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.WriteRecords(auxiliaryData);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(typeof(PredicationContext).FullName);
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
