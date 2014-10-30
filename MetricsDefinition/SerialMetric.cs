using System;

namespace MetricsDefinition
{
    public abstract class SerialMetric
    {
        private readonly int _windowSize;

        protected int WindowSize { get { return _windowSize; } }

        protected SerialMetric(int windowSize)
        {
            if (windowSize < 0)
            {
                throw new ArgumentOutOfRangeException("windowSize must be greater than or equal to 0");
            }

            _windowSize = windowSize;
        }
    }
}
