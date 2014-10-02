using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EvaluatorCmdClient
{
    sealed class EvaluationResultContextManager : IDisposable
    {
        private const string DefaultEvaluationName = "EvaluationResult";
        private const string ResultSummaryFileName = "Summary.txt";

        private TextWriter _resultSummaryWriter;
        private string _evaluationName;
        private string _rootDirectory;

        private int _contextId = 0;
        private List<EvaluationResultContext> _contexts = new List<EvaluationResultContext>();

        public EvaluationResultContextManager(string evaluationName)
        {
            if (string.IsNullOrWhiteSpace(evaluationName))
            {
                evaluationName = DefaultEvaluationName;
            }

            _evaluationName = evaluationName + "_" + DateTime.Now.ToString("yyyyMMddTHH:mm:ss");

            _rootDirectory = Path.GetFullPath(evaluationName);
            if (!Directory.Exists(_rootDirectory))
            {
                Directory.CreateDirectory(_rootDirectory);
            }

            string resultSummaryFile = Path.Combine(_rootDirectory, ResultSummaryFileName);
            _resultSummaryWriter = new StreamWriter(resultSummaryFile, false, Encoding.UTF8);
        }
    
        public EvaluationResultContext CreateNewContext(string contextAnnotation)
        {
            ++_contextId;
 
            string contextRootDirectory = Path.Combine(_rootDirectory, _contextId.ToString());
            var context = new EvaluationResultContext(contextRootDirectory, contextAnnotation);

            _contexts.Add(context);

            _resultSummaryWriter.WriteLine("{0},{1},{2}", _contextId, contextRootDirectory, contextAnnotation);

            return context;
        }

        public void Dispose()
        {
            if (_resultSummaryWriter != null)
            {
                _resultSummaryWriter.Flush();
                _resultSummaryWriter.Dispose();

                _resultSummaryWriter = null;
            }

            foreach (var context in _contexts)
            {
                context.Dispose();
            }

            _contexts.Clear();

            GC.SuppressFinalize(this);
        }
    }
}
