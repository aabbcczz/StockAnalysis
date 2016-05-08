using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public static class LinearRegression
    {
        public sealed class FinalResult
        {
            public double Slope { get; set; }
            public double Intercept { get; set; }
            public double CorrelationCoefficient { get; set; }
            public double SquareStandardError { get; set; }
            public double StdErrorForSlope { get; set; }
            public double StdErrorForIntercept { get; set; }

        }

        public sealed class IntermediateResult
        {
            public double N { get; private set; }
            public double SumX { get; private set; }
            public double SumY {get; private set; }
            public double SumXSquare {get; private set; }
            public double SumYSquare {get; private set; }
            public double SumXY { get; private set; }

            // add a point
            public void Add(double x, double y)
            {
                N += 1.0;
                SumX += x;
                SumY += y;
                SumXSquare += x * x;
                SumYSquare += y * y;
                SumXY += x * y;
            }

            // remove a point. caller needs to ensure the point has been added before.
            public void Remove(double x, double y)
            {
                N -= 1.0;
                SumX -= x;
                SumY -= y;
                SumXSquare -= x * x;
                SumYSquare -= y * y;
                SumXY -= x * y;
            }

            // shift all points on X axis, original point (x, y) becomes (x + deltaX, y)
            public void ShiftX(double deltaX)
            {
                SumXSquare += 2 * deltaX * SumX + N * deltaX * deltaX;
                SumXY += deltaX * SumY;
                SumX += N * deltaX;
            }

            // shift all points on Y axis. original point (x, y) becomes (x, y + deltaY)
            public void ShiftY(double deltaY)
            {
                SumYSquare += 2 * deltaY * SumY + N * deltaY * deltaY;
                SumXY += deltaY * SumX;
                SumY += N * deltaY;
            }

            // scale all points on X axis. original point (x, y) becomes (x * scaleX, y)
            public void ScaleX(double scaleX)
            {
                SumX *= scaleX;
                SumXSquare *= scaleX * scaleX;
                SumXY *= scaleX;
            }

            // scale all points on Y axis. original point (x, y) becomes (x, y * scaleY)
            public void ScaleY(double scaleY)
            {
                SumY *= scaleY;
                SumYSquare *= scaleY * scaleY;
                SumXY *= scaleY;
            }

            public FinalResult Compute()
            {
                return LinearRegression.Compute(N, SumX, SumY, SumXSquare, SumYSquare, SumXY);
            }
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
        //      s^2 = (SSyy - a * SSxy) / (n - 2)
        //      SE(a) = s * sqrt((1/n) + avg(x) * avg(x) / SSxx)
        //      SE(b) = s / sqrt(SSxx)

        public static FinalResult Compute(
            double n,
            double sumX,
            double sumY,
            double sumXSquare,
            double sumYSquare,
            double sumXY)
        {
#if DEBUG
            if (Math.Abs(n) < 1e-6)
            {
                throw new DivideByZeroException();
            }
#endif
            double avgX = sumX / n;
            double avgY = sumY / n;

            double SSxx = sumXSquare - n * avgX * avgX;
            double SSyy = sumYSquare - n * avgY * avgY;
            double SSxy = sumXY - n * avgX * avgY;

#if DEBUG
            if (Math.Abs(SSxx) < 1e-6 || Math.Abs(SSyy) < 1e-6)
            {
                throw new DivideByZeroException();
            }
#endif
            double slope = SSxy / SSxx;
            double intercept = avgY - slope * avgX;
            double correlationCoefficient = SSxy * SSxy / SSxx / SSyy;
            double squareStandardError = Math.Abs(n - 2.0) < 1e-6 ? 0.0 : (SSyy - slope * SSxy) / (n - 2.0);
            double standardError = Math.Sqrt(squareStandardError);
            double standardErrorForIntercept = standardError * Math.Sqrt(1.0 / n + avgX * avgX / SSxx);
            double standardErrorForSlope = standardError / Math.Sqrt(SSxx);

            return new FinalResult
            {
                Slope = slope,
                Intercept = intercept,
                CorrelationCoefficient = correlationCoefficient,
                SquareStandardError = squareStandardError,
                StdErrorForIntercept = standardErrorForIntercept,
                StdErrorForSlope = standardErrorForSlope,
            };
        }

        /// <summary>
        /// Calculate the regression of (x[0], y[0]), ..., (x[n-1], y[n-1])
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static FinalResult Compute(double[] x, double[] y)
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

            return Compute(n, sumX, sumY, sumXSquare, sumYSquare, sumXY);
        }

        /// <summary>
        /// Calculate the regression of (0, y[0]), ..., (n-1, y[n-1])
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static FinalResult ComputeSeries(double[] y)
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

            return Compute(n, sumX, sumY, sumXSquare, sumYSquare, sumXY);
        }

        /// <summary>
        /// Calculate the regression of (0, y1[0]), (0, y2[0])..., (n-1, y1[n-1]), (n-1, y2[n-1])
        /// </summary>
        public static FinalResult ComputeSeries(double[] y1, double[] y2)
        {
            if (y1 == null || y1.Length <= 1 || y2 == null || y2.Length <= 1)
            {
                throw new ArgumentNullException();
            }

            if (y1.Length != y2.Length)
            {
                throw new ArgumentException("Length of y1 and y2 are different");
            }

            double m = (double)y1.Length;
            double n = m * 2.0;
            double sumX = m * (m - 1.0);
            double sumXSquare = m * (m - 1.0) * (2 * m - 1.0) / 3.0;
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

            return Compute(n, sumX, sumY, sumXSquare, sumYSquare, sumXY);
        }
    }
}
