using System.Threading;

namespace StockAnalysis.TradingStrategy
{
    public static class IdGenerator
    {
        private static long _nextId;

        public static long Next
        {
            get { return Interlocked.Increment(ref _nextId); }
        }

        public static void Reset()
        {
            _nextId = 0;
        }
    }
}
