using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using CsvHelper;

namespace EvaluatorCmdClient
{
    sealed class EvaluationResultContextManager : IDisposable
    {
        private const string DefaultEvaluationName = "Evaluation";
        private const string ResultSummaryFileName = "ResultSummary.csv";
        private const string EvaluationSummaryFileName = "EvaluationSummary.xml";

        private string _evaluationName;
        private string _rootDirectory;
        private List<ResultSummary> _resultSummaries = new List<ResultSummary>();

        private int _contextId = 0;

        private bool _disposed = false;

        public EvaluationResultContextManager(string evaluationName)
        {
            if (string.IsNullOrWhiteSpace(evaluationName))
            {
                evaluationName = DefaultEvaluationName;
            }

            _evaluationName = evaluationName + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss");

            _rootDirectory = Path.GetFullPath(_evaluationName);
            if (!Directory.Exists(_rootDirectory))
            {
                Directory.CreateDirectory(_rootDirectory);
            }
        }

        public void SaveEvaluationSummary(EvaluationSummary summary)
        {
            string summaryFile = Path.Combine(_rootDirectory, EvaluationSummaryFileName);

            using (StreamWriter writer = new StreamWriter(summaryFile, false, Encoding.UTF8))
            {
                XmlSerializer serializer = new XmlSerializer(summary.GetType());
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
            string resultSummaryFile = Path.Combine(_rootDirectory, ResultSummaryFileName);
            
            using (StreamWriter writer = new StreamWriter(resultSummaryFile, false, Encoding.UTF8))
            {
                using (CsvWriter csvWriter = new CsvWriter(writer))
                {
                    csvWriter.Configuration.RegisterClassMap<ResultSummary.ResultSummaryMap>();
                    csvWriter.WriteRecords(_resultSummaries);
                }
            }
        }

        public EvaluationResultContext CreateNewContext()
        {
            ++_contextId;
 
            string contextRootDirectory = Path.Combine(_rootDirectory, _contextId.ToString());
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

            GC.SuppressFinalize(this);

            _disposed = true;
        }
    }
}
