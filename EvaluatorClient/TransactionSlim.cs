using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;

namespace EvaluatorClient
{
    sealed class TransactionSlim
    {
        public DateTime Time { get; private set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public string Action { get; private set; }
        public double Price { get; private set; }
        public int Volume { get; private set; }
        public string Comments { get; private set; }

        public TransactionSlim(Transaction transaction)
        {
            Time = transaction.ExecutionTime;
            Code = transaction.Code;
            switch(transaction.Action)
            {
                case TradingAction.OpenLong:
                    Action = "买入";
                    break;
                case TradingAction.CloseLong:
                    Action = "卖出";
                    break;
                default:
                    Action = "未知";
                    break;
            }

            Price = transaction.Price;
            Volume = transaction.Volume;
            Comments = transaction.Comments;
        }
    }
}
