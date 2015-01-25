using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalysis.Share;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace StockAnalysis.Share.Tests
{
    [TestClass()]
    public class LinearRegressionTests
    {
        [TestMethod()]
        public void ComputeTest()
        {
            double[] x = new double[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            };

            double[] y = new double[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            };

            var result = LinearRegression.Compute(x, y);
            Assert.AreEqual(result.Intercept, 0.0);
            Assert.AreEqual(result.Slope, 1.0);
            Assert.AreEqual(result.CorrelationCoefficient, 1.0);
            Assert.AreEqual(result.SquareStandardError, 0.0);
        }

        [TestMethod()]
        public void ComputeTest1()
        {
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void ComputeSeriesTest()
        {
            double[] y = new double[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            };

            var result = LinearRegression.ComputeSeries(y);
            Assert.AreEqual(result.Intercept, 1.0);
            Assert.AreEqual(result.Slope, 1.0);
            Assert.AreEqual(result.CorrelationCoefficient, 1.0);
            Assert.AreEqual(result.SquareStandardError, 0.0);
        }

        [TestMethod()]
        public void ComputeSeriesTest1()
        {
            double[] y1 = new double[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            };

            double[] y2 = new double[]
            {
                3, 4, 5, 6, 7, 8, 9, 10, 11, 12
            };

            var result = LinearRegression.ComputeSeries(y1, y2);
            Assert.AreEqual(result.Intercept, 2.0);
            Assert.AreEqual(result.Slope, 1.0);
            Assert.IsTrue(result.CorrelationCoefficient - 0.891891892 < 1e-6);
            Assert.IsTrue(result.SquareStandardError - 20.0 / 18.0 < 1e-6);
        }
    }
}
