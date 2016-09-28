using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class UntradableObject
    {
        private static string[] untradableCodes = new string[]
        {
            StockBoardIndex.GrowingBoardIndex,
            StockBoardIndex.MainBoardIndex,
            StockBoardIndex.SmallMiddleBoardIndex
        };

        private static Dictionary<string, UntradableObject> untradableObjects;

        public string Code { get; private set; }

        public UntradableObject(string code)
        {
            Code = code;
        }

        static UntradableObject()
        {
            untradableObjects = untradableCodes.ToDictionary(s => StockName.GetCanonicalCode(s), s => new UntradableObject(s));
        }

        public static bool IsUntradable(string code)
        {
            return untradableObjects.ContainsKey(code);
        }
    }
}
