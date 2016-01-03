using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RealTrading
{
    static class TradingEnvironment
    {
        private static int InitializationCount = 0;
        private static object SyncObj = new object();

        public static void Initialize()
        {
            lock (SyncObj)
            {
                if (InitializationCount < 0)
                {
                    throw new InvalidOperationException("unknown state");
                }

                if (InitializationCount == 0)
                {
                    TdxWrapper.OpenTdx();
                }

                ++InitializationCount;
            }
        }

        public static void UnInitialize()
        {
            lock (SyncObj)
            {
                if (InitializationCount <= 0)
                {
                    throw new InvalidOperationException("can't uninitialize environment that has not been initialized");
                }

                --InitializationCount;
                if (InitializationCount == 0)
                {
                    TdxWrapper.CloseTdx();
                }
            }
        }
    }
}
