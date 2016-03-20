using System;
using System.Collections.Generic;
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

        public string Name { get; set; }

        public TradingAction Action { get; set; }

        public SellingType SellingType { get; set; }

        public long Volume { get; set; }

        public double Price { get; set; }

        /// <summary>
        /// The stop loss gap (always smaller than or equal to 0.0) for buying. It means the price of buying plus
        /// the stop loss gap will be the stop loss price of the position.
        /// </summary>
        public double StopLossGapForBuying { get; set; }

        /// <summary>
        /// the stop loss price for selling, all positions that has stop loss price higher than this should be sold.
        /// this field is used only when Action is CloseLong.
        /// </summary>
        public double StopLossPriceForSelling { get; set; }

        /// <summary>
        /// the id of position for sell. 
        /// this field is used only one Action is CloseLong.
        /// </summary>
        public long PositionIdForSell { get; set; }

        public double Commission { get; set; }

        public string Error { get; set; }

        public string Comments { get; set; }

        public object[] RelatedObjects { get; set; }

        public string Print()
        {
            return string.Format(
                "{0},{1:u},{2:u}, {3}, {4}, {5}, {6}, {7:0.00}, {8:0.00}, {9:0.00}, {10:0.00}, {11},{12}",
                InstructionId,
                SubmissionTime,
                ExecutionTime,
                (int)Action,
                (int)SellingType,
                Code,
                Name,
                Price,
                Volume,
                Commission,
                Succeeded,
                Error.EscapeForCsv(),
                Comments.EscapeForCsv());
        }

        public Transaction()
        {
        }

        public Transaction(Instruction instruction, double price)
        {
            Action = instruction.Action;
            Commission = 0.0;
            ExecutionTime = default(DateTime);
            InstructionId = instruction.Id;
            Code = instruction.TradingObject.Code;
            Name = instruction.TradingObject.Name;
            Succeeded = false;
            Price = price;
            SubmissionTime = instruction.SubmissionTime;
            Volume = instruction.Volume;
            Comments = instruction.Comments;
            RelatedObjects = instruction.RelatedObjects;

            CloseInstruction closeInstruction = instruction as CloseInstruction;
            if (closeInstruction != null)
            {
                SellingType = closeInstruction.SellingType;
                StopLossPriceForSelling = closeInstruction.StopLossPriceForSelling;
                PositionIdForSell = closeInstruction.PositionIdForSell;
            }

            OpenInstruction openInstruction = instruction as OpenInstruction;
            if (openInstruction != null)
            {
                StopLossGapForBuying = openInstruction.StopLossGapForBuying;
            }
        }
    }
}
