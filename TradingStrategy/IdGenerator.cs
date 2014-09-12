using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public static class IdGenerator
    {
        private static long _nextId = 0;

        public static long Next
        {
            get { return System.Threading.Interlocked.Increment(ref _nextId); }
        }

        public static void Reset()
        {
            _nextId = 0;
        }
    }
}
