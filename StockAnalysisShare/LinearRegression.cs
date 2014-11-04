using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public static class LinearRegression
    {
        public sealed class RegressionResult
        {
            public double Slope { get; set; }
            public double Intercept { get; set; }
            public double ResidualSquare { get; set; }
        }

        public sealed class RegressionState
        {
            public double N { get; set; }
            public double SumX { get; set; }
            public double SumY {get; set; }
            public double SumXSquare {get; set; }
            public double SumYSquare {get; set; }
            public double SumXY { get; set; }
        }

        // Refer to http://mathworld.wolfram.com/LeastSquaresFitting.html
        // assume the fit is y = ax + b;
        // let:
        //      SSxx = sum(x^2) - n * avg(x)^2
        //      SSyy = sum(y^2) - n * avg(y)^2
        //      SSxy = sum(x * y) - n * avg(x) * avg(y)
        // then:
        //      a = SSxy / SSxx
        //      b = avg(y) - a * avg(x)
        //      r^2 = SSxy^2 / (SSxx * SSyy)

        private static RegressionResult FinalStep(
            double n,
            double sumX,
            double sumY,
            double sumXSquare,
            double sumYSquare,
            double sumXY)
        {
            double avgX = sumX / n;
            double avgY = sumY / n;

            double SSxx = sumXSquare - n * avgX * avgX;
            double SSyy = sumYSquare - n * avgY * avgY;
            double SSxy = sumXY - n * avgX * avgY;

            if (Math.Abs(SSxx) < 1e-10)
            {
                throw new OverflowException("Regression can't be calculate because SSxx is 0.0");
            }

            double slope = SSxy / SSxx;
            double intercept = avgY - slope * avgX;
            double residualSquare = SSxy * SSxy / SSxx / SSyy;

            return new RegressionResult
            {
                Slope = slope,
                Intercept = intercept,
                ResidualSquare = residualSquare
            };
        }

        public RegressionResult Compute(RegressionState state)
        {
            return FinalStep(state.N, state.SumX, state.SumY, state.SumXSquare, state.SumYSquare, state.SumXY);
        }

        /// <summary>
        /// Calculate the regression of (x[0], y[0]), ..., (x[n-1], y[n-1])
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static RegressionResult Compute(double[] x, double[] y)
        {
            if (x == null || y == null || x.Length <= 1 || y.Length <= 1)
            {
                throw new ArgumentNullException();
            }

            if (x.Length != y.Length)
            {
                throw new ArgumentException("length of x and y vector are different");
            }

            double sumX = 0.0;
            double sumXSquare = 0.0;
            double sumY = 0.0;
            double sumYSquare = 0.0;
            double sumXY = 0.0;
            double n = (double)x.Length;

            unchecked
            {
                for (int i = 0; i < x.Length; ++i)
                {
                    double xv = x[i];
                    double yv = y[i];

                    sumX += xv;
                    sumY += yv;
                    sumXSquare += xv * xv;
                    sumYSquare += yv * yv;
                    sumXY += xv * yv;
                }
            }

            return FinalStep(n, sumX, sumY, sumXSquare, sumYSquare, sumXY);
        }

        /// <summary>
        /// Calculate the regression of (0, y[0]), ..., (n-1, y[n-1])
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static RegressionResult Compute(double[] y)
        {
            if (y == null || y.Length <= 1)
            {
                throw new ArgumentNullException();
            }

            double n = (double)y.Length;
            double sumX = n * (n - 1.0) / 2.0;
            double sumXSquare = n * (n - 1.0) * (2 * n - 1.0) / 6.0;
            double sumY = 0.0;
            double sumYSquare = 0.0;
            double sumXY = 0.0;

            unchecked
            {
                for (int i = 0; i < y.Length; ++i)
                {
                    double yv = y[i];

                    sumY += yv;
                    sumYSquare += yv * yv;
                    sumXY += i * yv;
                }
            }

            return FinalStep(n, sumX, sumY, sumXSquare, sumYSquare, sumXY);
        }

        /// <summary>
        /// Calculate the regression of (0, y1[0]), (0, y2[0])..., (n-1, y1[n-1]), (n-1, y2[n-1])
        /// </summary>
        public static RegressionResult Compute(double[] y1, double[] y2)
        {
            if (y1 == null || y1.Length <= 1 || y2 == null || y2.Length <= 1)
            {
                throw new ArgumentNullException();
            }

            if (y1.Length != y2.Length)
            {
                throw new ArgumentException("Length of y1 and y2 are different");
            }

            double n = (double)y1.Length * 2.0;
            double sumX = n * (n - 1.0);
            double sumXSquare = n * (n - 1.0) * (2 * n - 1.0) / 3.0;
            double sumY = 0.0;
            double sumYSquare = 0.0;
            double sumXY = 0.0;

            unchecked
            {
                for (int i = 0; i < y1.Length; ++i)
                {
                    double y1v = y1[i];
                    double y2v = y2[i];

                    sumY += y1v + y2v;
                    sumYSquare += y1v * y1v + y2v * y2v;
                    sumXY += i * (y1v + y2v);
                }
            }

            return FinalStep(n, sumX, sumY, sumXSquare, sumYSquare, sumXY);
        }
    }
}
