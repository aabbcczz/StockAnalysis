using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;

namespace StockAnalysis.Share
{
    public static class Logger
    {
        public static ILog DebugLogger { get; private set; }
        public static ILog InfoLogger { get; private set; }
        public static ILog WarningLogger { get; private set; }
        public static ILog ErrorLogger { get; private set; }
        public static ILog FatalLogger { get; private set; }

        static Logger()
        {
            DebugLogger = LogManager.GetLogger("Debug");
            InfoLogger = LogManager.GetLogger("Info");
            WarningLogger = LogManager.GetLogger("Warning");
            ErrorLogger = LogManager.GetLogger("Error");
            FatalLogger = LogManager.GetLogger("Fatal");
        }
    }
}
