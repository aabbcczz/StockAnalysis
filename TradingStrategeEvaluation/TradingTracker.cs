using System;
using System.Collections.Generic;
using TradingStrategy;

namespace TradingStrategyEvaluation
{
    public sealed class TradingTracker
    {
        private readonly List<Transaction> _transactionHistory = new List<Transaction>();

        private readonly List<CompletedTransaction> _completedTransactionHistory = new List<CompletedTransaction>();

        public double InitialCapital { get; private set; }

        public DateTime MinTransactionTime { get; private set; }

        public DateTime MaxTransactionTime { get; private set; }

        public IEnumerable<Transaction> TransactionHistory 
        { 
            get 
            {
                return _transactionHistory; 
            } 
        }

        public IEnumerable<CompletedTransaction> CompletedTransactionHistory
        {
            get
            {
                return _completedTransactionHistory;
            }
        }

        public TradingTracker(double initialCapital)
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
            _transactionHistory.Add(transaction);

            if (transaction.ExecutionTime < MinTransactionTime)
            {
                MinTransactionTime = transaction.ExecutionTime;
            }

            if (transaction.ExecutionTime > MaxTransactionTime)
            {
                MaxTransactionTime = transaction.ExecutionTime;
            }

        }

        public void AddCompletedTransaction(CompletedTransaction completedTransaction)
        {
            _completedTransactionHistory.Add(completedTransaction);
        }
    }
}
