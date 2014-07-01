using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class EquityManager
    {
        public enum SellingSequence
        {
            FIFO,
            LIFO
        }

        private Dictionary<string, List<Equity>> _equities = new Dictionary<string, List<Equity>>();

        public readonly double InitialCapital { get; private set; }

        public double CurrentCapital { get; private set; }

        public SellingSequence Sequence { get; private set; }

        public EquityManager(double initialCapital, SellingSequence sequence)
        {
            InitialCapital = initialCapital;
            CurrentCapital = initialCapital;
            Sequence = sequence;
        }

        public bool ExecuteTransaction(Transaction transaction, out string error)
        {
            error = string.Empty;

            return false;
        }

        public IEnumerable<Equity> GetEquityDetails(string code)
        {
            return _equities[code].ToArray();
        }

        public IEnumerable<string> GetAllEquityCodes()
        {
            return _equities.Keys.ToArray();
        }
    }
}
