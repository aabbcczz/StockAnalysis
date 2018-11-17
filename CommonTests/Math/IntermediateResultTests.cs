using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalysis.Common.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Common.Math.Tests
{
    [TestClass()]
    public class IntermediateResultTests
    {
        [TestMethod()]
        public void AddTest()
        {
            var ir = new LinearRegression.IntermediateResult();
            ir.Add(0.0, 1.0);
            ir.Add(1.0, 2.0);
            ir.Add(2.0, 3.0);

            var result = ir.Compute();

            Assert.AreEqual(result.Intercept, 1.0);
            Assert.AreEqual(result.Slope, 1.0);
            Assert.AreEqual(result.CorrelationCoefficient, 1.0);
            Assert.AreEqual(result.SquareStandardError, 0.0);
        }

        [TestMethod()]
        public void RemoveTest()
        {
            var ir = new LinearRegression.IntermediateResult();
            ir.Add(0.0, 1.0);
            ir.Add(1.0, 2.0);
            ir.Add(2.0, 3.0);
            ir.Add(3.0, 4.0);

            var result = ir.Compute();

            Assert.AreEqual(result.Intercept, 1.0);
            Assert.AreEqual(result.Slope, 1.0);
            Assert.AreEqual(result.CorrelationCoefficient, 1.0);
            Assert.AreEqual(result.SquareStandardError, 0.0);

            ir.Remove(0.0, 1.0);

            var result1 = ir.Compute();

            Assert.AreEqual(result1.Intercept, 1.0);
            Assert.AreEqual(result1.Slope, 1.0);
            Assert.AreEqual(result1.CorrelationCoefficient, 1.0);
            Assert.AreEqual(result1.SquareStandardError, 0.0);
        }

        [TestMethod()]
        public void ShiftXTest()
        {
            var ir = new LinearRegression.IntermediateResult();
            ir.Add(1.0, 2.0);
            ir.Add(2.0, 3.0);
            ir.Add(3.0, 4.0);

            ir.ShiftX(-1.0);

            var result = ir.Compute();

            Assert.AreEqual(result.Intercept, 2.0);
            Assert.AreEqual(result.Slope, 1.0);
            Assert.AreEqual(result.CorrelationCoefficient, 1.0);
            Assert.AreEqual(result.SquareStandardError, 0.0);
        }

        [TestMethod()]
        public void ShiftYTest()
        {
            var ir = new LinearRegression.IntermediateResult();
            ir.Add(1.0, 2.0);
            ir.Add(2.0, 3.0);
            ir.Add(3.0, 4.0);

            ir.ShiftY(-1.0);

            var result = ir.Compute();

            Assert.AreEqual(result.Intercept, 0.0);
            Assert.AreEqual(result.Slope, 1.0);
            Assert.AreEqual(result.CorrelationCoefficient, 1.0);
            Assert.AreEqual(result.SquareStandardError, 0.0);
        }

        [TestMethod()]
        public void ScaleXTest()
        {
            var ir = new LinearRegression.IntermediateResult();
            ir.Add(1.0, 2.0);
            ir.Add(2.0, 4.0);
            ir.Add(3.0, 6.0);

            ir.ScaleX(2.0);

            var result = ir.Compute();

            Assert.AreEqual(result.Intercept, 0.0);
            Assert.AreEqual(result.Slope, 1.0);
            Assert.AreEqual(result.CorrelationCoefficient, 1.0);
            Assert.AreEqual(result.SquareStandardError, 0.0);
        }

        [TestMethod()]
        public void ScaleYTest()
        {
            var ir = new LinearRegression.IntermediateResult();
            ir.Add(2.0, 1.0);
            ir.Add(4.0, 2.0);
            ir.Add(6.0, 3.0);

            ir.ScaleY(2.0);

            var result = ir.Compute();

            Assert.AreEqual(result.Intercept, 0.0);
            Assert.AreEqual(result.Slope, 1.0);
            Assert.AreEqual(result.CorrelationCoefficient, 1.0);
            Assert.AreEqual(result.SquareStandardError, 0.0);
        }
    }
}