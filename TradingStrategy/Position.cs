using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class Position
    {
        public bool IsInitialized { get; private set; }

        public string Code { get; private set; }

        public DateTime BuyTime { get; private set; }

        public DateTime SellTime { get; private set; }

        public TradingAction BuyAction { get; private set; }

        public TradingAction SellAction { get; private set; }

        public int Volume { get; private set; }

        public double BuyPrice { get; private set; }

        public double SellPrice { get; private set; }

        public double BuyCommission { get; private set; }

        public double SellCommission { get; private set; }

        // 初始风险，即 R
        public double InitialRisk { get; set; }

        // 止损价格
        public double StopLossPrice { get; set; }

        public Position()
        {
        }

        public Position(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException();
            }

            switch (transaction.Action)
            {
                case TradingAction.OpenLong:
                    BuyTime = transaction.ExecutionTime;
                    Code = transaction.Code;
                    BuyAction = transaction.Action;
                    Volume = transaction.Volume;
                    BuyPrice = transaction.Price;
                    BuyCommission = transaction.Commission;
                    IsInitialized = true;
                    break;
                default:
                    throw new ArgumentException(string.Format("unsupported action {0}", transaction.Action));
            } 
        }

        public void Close(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException();
            }

            if (!IsInitialized)
            {
                throw new ArgumentException("uninitialized position can't be closed");
            }

            if (transaction.Code != Code)
            {
                throw new ArgumentException("code does not match");
            }

            switch (transaction.Action)
            {
                case TradingAction.CloseLong:
                    if (!IsInitialized)
                    {
                        throw new ArgumentException("postion is not initialized");
                    }

                    if (Volume != transaction.Volume)
                    {
                        throw new ArgumentException("volume does not match");
                    }

                    SellTime = transaction.ExecutionTime;
                    SellAction = transaction.Action;
                    SellPrice = transaction.Price;
                    SellCommission = transaction.Commission;

                    break;

                default:
                    throw new ArgumentException(string.Format("unsupported action {0}", transaction.Action));
            }
        }
    }
}
