using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    sealed class PositionTracer
    {
        public class Position
        {
            public string Code { get; private set; }

            public double Price { get; private set; }

            public int Volume { get; private set; }

            public Position(string code, double price, int volume)
            {
                if (string.IsNullOrEmpty(code)
                    || price <= 0.0
                    || volume <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                Code = code;
                Price = price;
                Volume = volume;
            }

            public void SubstractVolume(int volume)
            {
                if (volume < 0 || volume > Volume)
                {
                    throw new ArgumentOutOfRangeException();
                }

                Volume -= volume;
            }
        }

        public string Code { get; private set; }

        public double StopLoss { get; private set; }

        public double InitialRisk { get; private set; }

        public IEnumerable<Position> Positions
        {
            get { return _positions;  }
        }

        public int Count
        {
            get { return _positions.Count; }
        }

        private List<Position> _positions = new List<Position>();

        public PositionTracer(string code)
        {
            Code = code;
            StopLoss = 0.0;
        }

        public void AddPosition(Position position, double stopLoss)
        {
            if (position == null)
            {
                throw new ArgumentNullException();
            }

            if (stopLoss < 0.0)
            {
                throw new ArgumentOutOfRangeException("stopLoss must not be negative");
            }

            if (_positions.Count == 0)
            {
                if (!(stopLoss < position.Price))
                {
                    throw new ArgumentOutOfRangeException("stopLoss can't be smaller than initial price");
                }

                InitialRisk = (position.Price - stopLoss) * position.Volume;
            }

            SetStopLoss(stopLoss);

            _positions.Add(position);
        }

        public void RemovPosition(int volume)
        {
            int totalVolume = _positions.Sum(p => p.Volume);

            if (volume < 0 || volume > totalVolume)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (volume == 0)
            {
                return;
            }

            List<Position> resultPostions = new List<Position>();

            if (volume == totalVolume)
            {
                _positions = resultPostions;
                return;
            }

            int remainingVolume = volume;
            foreach (var position in _positions)
            {
                if (position.Volume > remainingVolume)
                {
                    position.SubstractVolume(remainingVolume);
                    resultPostions.Add(position);

                    remainingVolume = 0;
                }
                else
                {
                    remainingVolume -= position.Volume;
                }
            }

            _positions = resultPostions;
        }

        public void SetStopLoss(double stopLoss)
        {
            if (stopLoss < StopLoss)
            {
                throw new ArgumentException("the price of stop loss can't be degraded");
            }

            StopLoss = stopLoss;
        }
    }
}
