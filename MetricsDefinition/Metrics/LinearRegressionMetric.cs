using System;
using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    /// <summary>
    /// The linear regression metric
    /// </summary>
    [Metric("LR", "SLOPE,INTERCEPT,SR,SSE")]
    public sealed class LinearRegressionMetric : MultipleOutputBarInputSerialMetric
    {
        // we always assume every day 10% of change equals to slope 1.0 (45 degree), so all data will be scaled by 10.0.
        private const double DefaultPriceScale = 10.0;

        private LinearRegression.IntermediateResult _intermediateResult = new LinearRegression.IntermediateResult();

        public LinearRegressionMetric(int windowSize)
            : base(windowSize)
        {
            if (windowSize < 2)
            {
                throw new ArgumentException("window size for linear regression metric must be greater than 1");
            }
        }

        public override double[] Update(Bar bar)
        {
            double scale;
            if (Data.Length < WindowSize)
            {
                // always normalize the first open price as (0, 10), and normalize other data points as (N, 10 * price / first open price)
                scale = Data.Length == 0 ? DefaultPriceScale / bar.OpenPrice : DefaultPriceScale / Data[0].OpenPrice;

                _intermediateResult.Add((double)Data.Length, bar.OpenPrice * scale);
                _intermediateResult.Add((double)Data.Length, bar.ClosePrice * scale);
                Data.Add(bar);
            }
            else
            {
                // existing data have been normalized, but newly added data will cause original (0, 1) being spoiled out,
                // so we need to normalize it again.

                var bar0 = Data[0];

                scale = DefaultPriceScale / bar0.OpenPrice;

                _intermediateResult.Remove(0.0, bar0.OpenPrice * scale);
                _intermediateResult.Remove(0.0, bar0.ClosePrice * scale);

                _intermediateResult.Add((double)WindowSize, bar.OpenPrice * scale);
                _intermediateResult.Add((double)WindowSize, bar.ClosePrice * scale);

                _intermediateResult.ShiftX(-1.0);

                // now we use the newly first point to normalize the points
                scale = bar0.OpenPrice / Data[1].OpenPrice;
                _intermediateResult.ScaleY(scale);

                Data.Add(bar);
            }

            if (Data.Length < 2)
            {
                return new double[] { 0.0, 0.0, 0.0, 0.0};
            }
            else
            {
                var result = _intermediateResult.Compute();

                return new double[] { result.Slope, result.Intercept, result.SquareResidual, result.SquareStandardError };
            }
        }
    }
}
