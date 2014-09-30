using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using TradingStrategy;

namespace TradingStrategyEvaluation
{
    public sealed class TradingHistory
    {
        private List<Transaction> _history = new List<Transaction>();

        public double InitialCapital { get; private set; }

        public DateTime MinTransactionTime { get; private set; }

        public DateTime MaxTransactionTime { get; private set; }

        public IEnumerable<Transaction> History 
        { 
            get 
            {
                return _history; 
            } 
        }

        public TradingHistory(double initialCapital)
        {
            if (initialCapital <= 0.0)
            {
                throw new ArgumentOutOfRangeException("initial capital must be greater than 0.0");
            }

            InitialCapital = initialCapital;

            MinTransactionTime = DateTime.MaxValue;
            MaxTransactionTime = DateTime.MinValue;
        }

        public void AddTransaction(Transaction transaction)
        {
            _history.Add(transaction);

            if (transaction.ExecutionTime < MinTransactionTime)
            {
                MinTransactionTime = transaction.ExecutionTime;
            }

            if (transaction.ExecutionTime > MaxTransactionTime)
            {
                MaxTransactionTime = transaction.ExecutionTime;
            }

        }

        public void SaveToFile(string file)
        {
            if (file == null || file == string.Empty)
            {
                throw new ArgumentNullException();
            }

            using(StreamWriter writer = new StreamWriter(file, false, Encoding.UTF8))
            {
                writer.WriteLine("{0:0.00}", InitialCapital);
                writer.WriteLine(Transaction.ToStringHeader);
                foreach (var t in History)
                {
                    writer.WriteLine(t.ToString());
                }
            }
        }

        public TradingHistory Clone()
        {
            TradingHistory history = new TradingHistory(InitialCapital);
            history._history.AddRange(_history);

            return history;
        }

        public static TradingHistory LoadFromFile(string file)
        {
            if (file == null || file == string.Empty)
            {
                throw new ArgumentNullException();
            }

            string[] lines = File.ReadAllLines(file, Encoding.UTF8)
                .Where(s => s != null && !string.IsNullOrWhiteSpace(s))
                .ToArray();

            if (lines.Length < 1)
            {
                throw new InvalidDataException("no data in file");
            }

            double initialCapital = double.Parse(lines[0]);

            TradingHistory history = new TradingHistory(initialCapital);

            for (int i = 1; i < lines.Length; ++i)
            {

                try
                {
                    Transaction transaction = Transaction.Parse(lines[i]);

                    // additional check
                    if (string.IsNullOrWhiteSpace(transaction.Code))
                    {
                        throw new FormatException("code is empty");
                    }

                    if (transaction.Price < 0.0)
                    {
                        throw new FormatException("price is smaller than 0.0");
                    }

                    if (transaction.Volume < 0)
                    {
                        throw new FormatException("volume is smaller than 0");
                    }

                    if (transaction.Commission < 0.0)
                    {
                        throw new FormatException("commission is smaller than 0");
                    }

                    history.AddTransaction(transaction);
                }
                catch (FormatException ex)
                {
                    throw new InvalidDataException(
                        string.Format("Data format error at line {0}\n{1}", i, lines[i]),
                        ex);
                }
            }

            return history;
        }
    }
}
