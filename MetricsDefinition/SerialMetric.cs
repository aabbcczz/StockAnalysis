namespace StockAnalysis.MetricsDefinition
{
    using System;

    public abstract class SerialMetric
    {
        private readonly int _windowSize;

        protected int WindowSize { get { return _windowSize; } }

        public double[] Values { get; protected set; }

        public double Value
        {
            get
            {
                unchecked
                {
                    return Values[0];
                }
            }
        }

        protected SerialMetric(int windowSize)
        {
            if (windowSize < 0)
            {
                throw new ArgumentOutOfRangeException("windowSize must be greater than or equal to 0");
            }

            _windowSize = windowSize;
        }

        protected void SetValue(double v0)
        {
            unchecked
            {
                Values[0] = v0;
            }
        }

        protected void SetValue(double v0, double v1)
        {
            unchecked
            {
                Values[0] = v0;
                Values[1] = v1;
            }
        }

        protected void SetValue(double v0, double v1, double v2)
        {
            unchecked
            {
                Values[0] = v0;
                Values[1] = v1;
                Values[2] = v2;
            }
        }
        protected void SetValue(double v0, double v1, double v2, double v3)
        {
            unchecked
            {
                Values[0] = v0;
                Values[1] = v1;
                Values[2] = v2;
                Values[3] = v3;
            }
        }
        protected void SetValue(double v0, double v1, double v2, double v3, double v4)
        {
            unchecked
            {
                Values[0] = v0;
                Values[1] = v1;
                Values[2] = v2;
                Values[3] = v3;
                Values[4] = v4;
            }
        }

        protected void SetValue(double v0, double v1, double v2, double v3, double v4, double v5)
        {
            unchecked
            {
                Values[0] = v0;
                Values[1] = v1;
                Values[2] = v2;
                Values[3] = v3;
                Values[4] = v4;
                Values[5] = v5;
            }
        }

        protected void SetValue(params double[] values)
        {
            unchecked
            {
                Array.Copy(values, Values, values.Length);
            }
        }
    }
}
