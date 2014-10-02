using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using TradingStrategy;
using TradingStrategyEvaluation;

namespace EvaluatorCmdClient
{
    sealed class EvaluationResultContext : IDisposable
    {
        private const string LogFileName = "Log.txt";

        private string _rootDirectory;
        private string _annotation;

        public ILogger Logger { get; private set; }

        public EvaluationResultContext(string rootDirectory, string annotation)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                throw new ArgumentNullException();
            }

            _rootDirectory = rootDirectory;
            _annotation = annotation;

            string logFile = Path.Combine(rootDirectory, LogFileName);
            Logger = new FileLogger(logFile);
        }

        public void Dispose()
        {
            if (Logger != null)
            {
                ((FileLogger)Logger).Dispose();
                Logger = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
