using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalysis.Share;

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

        public SellingType SellingType { get; set; }

        public int Volume { get; set; }

        public double Price { get; set; }

        /// <summary>
        /// the stop loss price for sell, all positions that has stop loss price higher than this should be sold.
        /// this field is used only when Action is CloseLong.
        /// </summary>
        public double StopLossPriceForSell { get; set; }

        /// <summary>
        /// the id of position for sell. 
        /// this field is used only one Action is CloseLong.
        /// </summary>
        public long PositionIdForSell { get; set; }

        public double Commission { get; set; }

        public string Error { get; set; }

        public string Comments { get; set; }

        public class DefaultComparer : IComparer<Transaction>
        {
                 
            public int Compare(Transaction x, Transaction y)
            {
 	            if (x.ExecutionTime != y.ExecutionTime)
                {
                    return x.ExecutionTime.CompareTo(y.ExecutionTime);
                }

                if (x.Action != y.Action)
                {
                    if (x.Action == TradingAction.CloseLong)
                    {
                        return 1;
                    }
                    else if (y.Action == TradingAction.CloseLong)
                    {
                        return -1;
                    }
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

        public override string ToString()
        {
            return string.Format(
                "{0},{1:yyyy-MM-dd HH:mm:ss},{2:yyyy-MM-dd HH:mm:ss}, {3}, {4}, {5}, {6:0.00}, {7:0.00}, {8:0.00}, {9:0.00}, {10},{11}",
                InstructionId,
                SubmissionTime,
                ExecutionTime,
                (int)Action,
                (int)SellingType,
                Code,
                Price,
                Volume,
                Commission,
                Succeeded,
                Error.Escape(),
                Comments.Escape());
        }

        public static Transaction Parse(string s)
        {
            string[] fields = s.Split(new char[] { ',' });
            if (fields.Length != 12)
            {
                // there should be 11 fields
                throw new FormatException(
                    string.Format("Data format error: there is no enough fields in \"{0}\"", s));

            }

            int fid = 0;
            Transaction transaction = new Transaction()
            {
                InstructionId = long.Parse(fields[fid++]),
                SubmissionTime = DateTime.Parse(fields[fid++]),
                ExecutionTime = DateTime.Parse(fields[fid++]),
                Action = (TradingAction)int.Parse(fields[fid++]),
                SellingType = (SellingType)int.Parse(fields[fid++]),
                Code = fields[fid++],
                Price = double.Parse(fields[fid++]),
                Volume = int.Parse(fields[fid++]),
                Commission = double.Parse(fields[fid++]),
                Succeeded = bool.Parse(fields[fid++]),
                Error = fields[fid++].Unescape(),
                Comments = fields[fid++].Unescape()
            };

            return transaction;
        }
    }
}
