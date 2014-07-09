using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class Transaction
    {
        public long InstructionId { get; set; }

        public bool Succeeded { get; set; }

        public DateTime SubmissionTime { get; set; }

        public DateTime ExecutionTime { get; set; }

        public string Code { get; set; }

        public TradingAction Action { get; set; }

        public int Volume { get; set; }

        public double Price { get; set; }

        public double Commission { get; set; }

        public string Error { get; set; }

        public class DefaultComparer : IComparer<Transaction>
        {
                 
            public int Compare(Transaction x, Transaction y)
            {
 	            if (x.ExecutionTime != y.ExecutionTime)
                {
                    return x.ExecutionTime.CompareTo(y.ExecutionTime);
                }

                if (x.SubmissionTime != y.SubmissionTime)
                {
                    return x.SubmissionTime.CompareTo(y.SubmissionTime);
                }

                if (x.Code != y.Code)
                {
                    return x.Code.CompareTo(y.Code);
                }

                if (x.InstructionId != y.InstructionId)
                {
                    return x.InstructionId.CompareTo(y.InstructionId);
                }

                return 0;
            }
        }
    }
}
