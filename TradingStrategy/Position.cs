using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class Position
    {
        public const double UninitializedStopLossPrice = double.MinValue;

        public long ID { get; set; }

        public bool IsInitialized { get; set; }

        public string Code { get; set; }

        public DateTime BuyTime { get; set; }

        public DateTime SellTime { get; set; }

        public TradingAction BuyAction { get; set; }

        public TradingAction SellAction { get; set; }

        public int Volume { get; set; }

        public double BuyPrice { get; set; }

        public double SellPrice { get; set; }

        public double BuyCommission { get; set; }

        public double SellCommission { get; set; }

        // 初始风险，即 R
        public double InitialRisk { get; set; }

        // 止损价格
        public double StopLossPrice { get; set; }

        public Position()
        {
            ID = IdGenerator.Next;

            InitialRisk = 0.0;
            StopLossPrice = UninitializedStopLossPrice;
        }

        public Position(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException();
            }

            ID = IdGenerator.Next;

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

            InitialRisk = 0.0;
            StopLossPrice = UninitializedStopLossPrice;
        }

        /// <summary>
        /// Split a position into two parts according the volume parameter.
        /// The existing position will include the volume specified in the parameter,
        /// and the new position object returned will include remaining volumes
        /// </summary>
        /// <param name="volume">expected volume kept in old position</param>
        /// <returns>new position that include remaining volumes</returns>
        public Position Split(int volume)
        {
            if (volume <= 0 || volume >= this.Volume)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (!IsInitialized)
            {
                throw new InvalidOperationException("Can't split uninitialized position");
            }

            double oldPositionPercentage = (double)volume / this.Volume;
            double newPositionPercentage = 1.0 - oldPositionPercentage;

            // create new position
            Position newPosition = new Position()
            {
                IsInitialized = this.IsInitialized,
                Code = this.Code,
                BuyTime = this.BuyTime,
                SellTime = this.SellTime,
                BuyAction = this.BuyAction,
                SellAction = this.SellAction,
                Volume = this.Volume - volume,
                BuyPrice = this.BuyPrice,
                SellPrice = this.SellPrice,
                BuyCommission = this.BuyCommission * newPositionPercentage,
                SellCommission = this.SellCommission * newPositionPercentage,
                InitialRisk = this.IsStopLossPriceInitialized() ? this.InitialRisk * newPositionPercentage : 0.0,
                StopLossPrice = this.StopLossPrice,
            };

            // update this position
            this.Volume -= volume;
            this.BuyCommission *= oldPositionPercentage;
            this.SellCommission *= oldPositionPercentage;
            this.InitialRisk = this.IsStopLossPriceInitialized() ? this.InitialRisk * oldPositionPercentage : 0.0;

            return newPosition;
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

        public bool IsStopLossPriceInitialized()
        {
            return StopLossPrice != UninitializedStopLossPrice;
        }

        public void SetStopLossPrice(double stopLossPrice)
        {
            if (stopLossPrice < 0.0 || stopLossPrice > BuyPrice)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (!IsStopLossPriceInitialized())
            { 
                StopLossPrice = stopLossPrice;

                InitialRisk = (BuyPrice - StopLossPrice) * Volume;
            }
            else
            {
                if (stopLossPrice < StopLossPrice)
                {
                    throw new InvalidOperationException("Can't reset stop loss price to smaller value");
                }

                StopLossPrice = stopLossPrice;
            }
        }
    }
}
