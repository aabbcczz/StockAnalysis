using System;

namespace TradingStrategy
{
    public sealed class Position
    {
        private const double UninitializedStopLossPrice = double.MinValue;

        public long Id { get; set; }

        public bool IsInitialized { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

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

        public double GainInR { get; set; }

        public string Comments { get; set; }

        public double MetricValue1 { get; set; }
        public double MetricValue2 { get; set; }
        public double MetricValue3 { get; set; }
        public double MetricValue4 { get; set; }

        public int LastedPeriodCount { get; private set; }

        public Position()
        {
            Id = IdGenerator.Next;

            InitialRisk = 0.0;
            StopLossPrice = UninitializedStopLossPrice;
            LastedPeriodCount = 0;
        }

        public Position(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException();
            }

            Id = IdGenerator.Next;

            switch (transaction.Action)
            {
                case TradingAction.OpenLong:
                    BuyTime = transaction.ExecutionTime;
                    Code = transaction.Code;
                    Name = transaction.Name;
                    BuyAction = transaction.Action;
                    Volume = transaction.Volume;
                    BuyPrice = transaction.Price;
                    BuyCommission = transaction.Commission;
                    IsInitialized = true;
                    Comments = transaction.Comments;

                    if (transaction.ObservedMetricValues != null && transaction.ObservedMetricValues.Length > 0)
                    {
                        MetricValue1 = transaction.ObservedMetricValues[0];

                        int length = transaction.ObservedMetricValues.Length;
                        if (length > 1)
                        {
                            MetricValue2 = transaction.ObservedMetricValues[1];
                        }

                        if (length > 2)
                        {
                            MetricValue3 = transaction.ObservedMetricValues[2];
                        }

                        if (length > 3)
                        {
                            MetricValue4 = transaction.ObservedMetricValues[3];
                        }
                    }
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
            if (volume <= 0 || volume >= Volume)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (!IsInitialized)
            {
                throw new InvalidOperationException("Can't split uninitialized position");
            }

            var oldPositionPercentage = (double)volume / Volume;
            var newPositionPercentage = 1.0 - oldPositionPercentage;

            // create new position
            var newPosition = new Position
            {
                IsInitialized = this.IsInitialized,
                Code = this.Code,
                Name = this.Name,
                BuyTime = this.BuyTime,
                SellTime = this.SellTime,
                BuyAction = this.BuyAction,
                SellAction = this.SellAction,
                Volume = this.Volume - volume,
                BuyPrice = this.BuyPrice,
                SellPrice = this.SellPrice,
                BuyCommission = this.BuyCommission * newPositionPercentage,
                SellCommission = this.SellCommission * newPositionPercentage,
                InitialRisk = IsStopLossPriceInitialized() ? this.InitialRisk * newPositionPercentage : 0.0,
                StopLossPrice = this.StopLossPrice,
                Comments = this.Comments,
                MetricValue1 = this.MetricValue1,
                MetricValue2 = this.MetricValue2,
                MetricValue3 = this.MetricValue3,
                MetricValue4 = this.MetricValue4,
                LastedPeriodCount = this.LastedPeriodCount,
            };

            // update this position
            Volume -= volume;
            BuyCommission *= oldPositionPercentage;
            SellCommission *= oldPositionPercentage;
            InitialRisk = IsStopLossPriceInitialized() ? InitialRisk * oldPositionPercentage : 0.0;

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
                    
                    if (!string.IsNullOrEmpty(transaction.Comments))
                    {
                        Comments += ";" + transaction.Comments;
                    }

                    GainInR = (SellPrice - BuyPrice) * Volume / InitialRisk;
                    break;

                default:
                    throw new ArgumentException(string.Format("unsupported action {0}", transaction.Action));
            }
        }

        public bool IsStopLossPriceInitialized()
        {
            return Math.Abs(StopLossPrice - UninitializedStopLossPrice) > 1e-6;
        }

        public void SetStopLossPrice(double stopLossPrice)
        {
            if (stopLossPrice < 0.0)
            {
                throw new ArgumentOutOfRangeException("stop loss price must be greater than 0.0");
            }

            if (!IsStopLossPriceInitialized())
            { 
                if (stopLossPrice > BuyPrice)
                {
                    throw new ArgumentOutOfRangeException("initial stop loss price can't be greater than buy price");
                }

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

        public void IncreaseLastedPeriodCount()
        {
            ++LastedPeriodCount;
        }
    }
}
