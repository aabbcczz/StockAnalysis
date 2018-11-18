namespace EvaluatorCmdClient
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.IO;
    using System.Xml.Serialization;

    using CsvHelper;
    sealed class EvaluationResultContextManager : IDisposable
    {
        private const string DefaultEvaluationName = "Evaluation";
        private const string ResultSummaryFileName = "ResultSummary.csv";
        private const string EvaluationSummaryFileName = "EvaluationSummary.xml";

        private readonly string _rootDirectory;
        private readonly List<ResultSummary> _resultSummaries = new List<ResultSummary>();

        private int _contextId;

        private bool _disposed;

        public EvaluationResultContextManager(string evaluationName)
        {
            if (string.IsNullOrWhiteSpace(evaluationName))
            {
                evaluationName = DefaultEvaluationName;
            }

            evaluationName = evaluationName + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss");

            _rootDirectory = Path.GetFullPath(evaluationName);
            if (!Directory.Exists(_rootDirectory))
            {
                Directory.CreateDirectory(_rootDirectory);
            }
        }

        public void SaveEvaluationSummary(EvaluationSummary summary)
        {
            var summaryFile = Path.Combine(_rootDirectory, EvaluationSummaryFileName);

            using (var writer = new StreamWriter(summaryFile, false, Encoding.UTF8))
            {
                var serializer = new XmlSerializer(summary.GetType());
                serializer.Serialize(writer, summary);
            }
        }
    
        public void AddResultSummary(ResultSummary summary)
        {
            if (summary == null)
            {
                throw new ArgumentNullException();
            }

            _resultSummaries.Add(summary);
        }

        public void SaveResultSummaries()
        {
            var resultSummaryFile = Path.Combine(_rootDirectory, ResultSummaryFileName);
            
            using (var writer = new StreamWriter(resultSummaryFile, false, Encoding.UTF8))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.Configuration.RegisterClassMap<ResultSummary.ResultSummaryMap>();
                    csvWriter.WriteRecords(_resultSummaries);
                }
            }
        }

        public EvaluationResultContext CreateNewContext()
        {
            ++_contextId;
 
            var contextRootDirectory = Path.Combine(_rootDirectory, _contextId.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(contextRootDirectory))
            {
                Directory.CreateDirectory(contextRootDirectory);
            }

            var context = new EvaluationResultContext(_contextId, contextRootDirectory);

            return context;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(typeof(EvaluationResultContextManager).FullName);
            }

            _disposed = true;
        }
    }
}
