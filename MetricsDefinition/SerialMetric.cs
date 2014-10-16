using System;

namespace MetricsDefinition
{
    public abstract class SerialMetric
    {
        private readonly int _windowSize;

        public int WindowSize { get { return _windowSize; } }

        public SerialMetric(int windowSize)
        {
            if (windowSize <= 0)
            {
                throw new ArgumentOutOfRangeException("windowSize must be greater than 0");
            }

            _windowSize = windowSize;
        }
    }
}
