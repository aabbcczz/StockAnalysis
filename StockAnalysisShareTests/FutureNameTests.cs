using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace StockAnalysis.Share.Tests
{
    [TestClass()]
    public class FutureNameTests
    {
        [TestMethod()]
        public void GetNameForProductSymbolTest()
        {
            Assert.AreEqual(FutureName.GetNameForProductSymbol("AL8"), "豆一主连");
            Assert.AreEqual(FutureName.GetNameForProductSymbol("A1611"), "豆一1611");

            var regex = new Regex(@"^(cu|al)\d{4}$", RegexOptions.IgnoreCase);
            Assert.IsTrue(regex.IsMatch("cu1234"));
            Assert.IsTrue(regex.IsMatch("Cu1234"));
            Assert.IsTrue(regex.IsMatch("al1234"));
            Assert.IsTrue(regex.IsMatch("AL1234"));
            Assert.IsTrue(regex.IsMatch("CU1234"));
            Assert.IsFalse(regex.IsMatch("CL1234"));
            Assert.IsFalse(regex.IsMatch("cu123"));
            Assert.IsFalse(regex.IsMatch("cua123"));
            Assert.IsFalse(regex.IsMatch("cu12345"));
        }
    }
}
