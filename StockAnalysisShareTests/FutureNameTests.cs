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
    public class FutureNameTests
    {
        [TestMethod()]
        public void GetNameForProductSymbolTest()
        {
            Assert.AreEqual(FutureName.GetNameForProductSymbol("AL8"), "豆一主连");
            Assert.AreEqual(FutureName.GetNameForProductSymbol("A1611"), "豆一1611");
        }
    }
}
