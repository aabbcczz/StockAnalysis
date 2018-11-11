using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class UntradableObject
    {
        private static string[] untradableSymbols = new string[]
        {
            StockBoardIndex.GrowingBoardIndexName.NormalizedSymbol,
            StockBoardIndex.MainBoardIndexName.NormalizedSymbol,
            StockBoardIndex.SmallMiddleBoardIndexName.NormalizedSymbol
        };

        private static Dictionary<string, UntradableObject> untradableObjects;

        public string Symbol { get; private set; }

        public UntradableObject(string symbol)
        {
            Symbol = symbol;
        }

        static UntradableObject()
        {
            untradableObjects = untradableSymbols.ToDictionary(s => s, s => new UntradableObject(s));
        }

        public static bool IsUntradable(string normalizedSymbol)
        {
            return untradableObjects.ContainsKey(normalizedSymbol);
        }
    }
}
