namespace StockAnalysis.Share
{
    using log4net;

    public static class AppLogger
    {
        public static string DefaultLoggerName = "Default";

        private static object _syncObj = new object();
        private static ILog _log = null;

        public static ILog Default 
        { 
            get
            {
                if (_log == null)
                {
                    lock (_syncObj)
                    {
                        if (_log == null)
                        {
                            _log = LogManager.GetLogger(DefaultLoggerName);
                        }
                    }
                }

                return _log;
            }
        }
    }
}
