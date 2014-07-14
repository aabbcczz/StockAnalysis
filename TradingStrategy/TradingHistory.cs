using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TradingStrategy
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

        public static void SaveToFile(TradingHistory history, string file)
        {
            if (file == null || file == string.Empty)
            {
                throw new ArgumentNullException();
            }

            using(StreamWriter writer = new StreamWriter(file, false, Encoding.UTF8))
            {
                writer.WriteLine("{0:0.00}", history.InitialCapital);
                writer.WriteLine("InstructionId,SubmissionTime,ExecutionTime,Action,Code,Price,Volume,Commission,Succeeded,Error");
                foreach (var t in history.History)
                {
                    writer.WriteLine(
                        "{0},{1:yyyy-MM-dd HH:mm:ss},{2:yyyy-MM-dd HH:mm:ss}, {3}, {4}, {5:0.00}, {6:0.00}, {7:0.00}, {8}, {9}",
                        t.InstructionId,
                        t.SubmissionTime,
                        t.ExecutionTime,
                        (int)t.Action,
                        t.Code,
                        t.Price,
                        t.Volume,
                        t.Commission,
                        t.Succeeded,
                        t.Error);
                }
            }
        }

        public TradingHistory Clone()
        {
            TradingHistory history = new TradingHistory(InitialCapital);
            history._history.AddRange(_history);

            return history;
        }

        public TradingHistory LoadFromFile(string file)
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
                string[] fields = lines[i].Split(new char[] { ',' });
                if (fields.Length < 10)
                {
                    // there should be 10 fields
                    throw new InvalidDataException(
                        string.Format("Data format error: there is no enough fields in line {0}\n{1}", i, lines[i]));

                }

                try
                {
                    Transaction transaction = new Transaction()
                    {
                        InstructionId = long.Parse(fields[0]),
                        SubmissionTime = DateTime.Parse(fields[1]),
                        ExecutionTime = DateTime.Parse(fields[2]),
                        Action = (TradingAction)int.Parse(fields[3]),
                        Code = fields[4],
                        Price = double.Parse(fields[5]),
                        Volume = int.Parse(fields[6]),
                        Commission = double.Parse(fields[7]),
                        Succeeded = bool.Parse(fields[8]),
                        Error = string.Join(",", fields.Skip(9)) // all remained fields are error message.
                    };

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
