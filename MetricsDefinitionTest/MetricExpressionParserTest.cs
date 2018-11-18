using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StockAnalysis.MetricsDefinition;

namespace StockAnalysis.MetricsDefinitionTest
{
    [TestClass]
    public class MetricExpressionParserTest
    {
        [TestMethod]
        public void TestParse()
        {
            var parser = new MetricExpressionParser();

            try
            {
                // null input
                parser.Parse(null);
            }
            catch (ArgumentNullException)
            {
                // pass
            }

            // empty input
            Assert.IsNull(parser.Parse(""));
            Assert.IsNull(parser.Parse("  \t\n"));

            // case sensitivity test
            Assert.IsNull(parser.Parse("ma[20]"));
            Assert.IsNotNull(parser.Parse("MA[20]"));

            // unknown metric name
            Assert.IsNull(parser.Parse("M"));

            // multiple metric name
            Assert.IsNotNull(parser.Parse("EMA[20]"));
            Assert.IsNotNull(parser.Parse("EXPMA[20]"));

            // invalid parameters
            Assert.IsNull(parser.Parse("MA"));
            Assert.IsNull(parser.Parse("MA[20,10]"));
            Assert.IsNull(parser.Parse("MA[20;10]"));
            Assert.IsNull(parser.Parse("MA[20(10]"));
            Assert.IsNull(parser.Parse("MA[20)10]"));
            Assert.IsNull(parser.Parse("MA[abc]"));
            Assert.IsNull(parser.Parse("MA[20.00]"));
            Assert.IsNull(parser.Parse("MA[.00]"));
            Assert.IsNull(parser.Parse("MA[-20]"));
            Assert.IsNull(parser.Parse("MA[+20]"));
            Assert.IsNull(parser.Parse("MA[20a]"));
            Assert.IsNull(parser.Parse("MA[20.00a]"));

            // valid parameters
            Assert.IsNotNull(parser.Parse("MA[20]"));
            Assert.IsNotNull(parser.Parse("MA[20] "));
            Assert.IsNotNull(parser.Parse("MA [20]"));
            Assert.IsNotNull(parser.Parse("MA[ 20 ]"));
            Assert.IsNotNull(parser.Parse("TESTMETRIC[20, \"abc\", 1.1]"));

            // valid syntax
            Assert.IsNotNull(parser.Parse("BAR"));
            Assert.IsNotNull(parser.Parse("BAR[]"));

            // invalid embed syntax
            Assert.IsNull(parser.Parse("BAR()"));
            Assert.IsNull(parser.Parse("MA[20]()"));
            Assert.IsNull(parser.Parse("BAR["));
            Assert.IsNull(parser.Parse("BAR]"));
            Assert.IsNull(parser.Parse("MA[20"));
            Assert.IsNull(parser.Parse("BAR,"));
            Assert.IsNull(parser.Parse("MA[20,"));

            Assert.IsNull(parser.Parse("BAR(BAR"));
            Assert.IsNull(parser.Parse("MA[20](BAR"));
            Assert.IsNull(parser.Parse("MA[20](MA[20]"));

            // valid embeded syntax
            Assert.IsNotNull(parser.Parse("BAR(BAR)"));
            Assert.IsNotNull(parser.Parse("MA[20](BAR)"));
            Assert.IsNotNull(parser.Parse("MA[20](MA[20])"));
            Assert.IsNotNull(parser.Parse("MA[20](MA[20](BAR))"));

            // invalid selection syntax
            Assert.IsNull(parser.Parse("MA[20]."));
            Assert.IsNull(parser.Parse("MA[20].X"));
            Assert.IsNull(parser.Parse("MA[20].1"));
            Assert.IsNull(parser.Parse("MA[20].("));
            Assert.IsNull(parser.Parse("MA[20].)"));
            Assert.IsNull(parser.Parse("MA[20].."));
            Assert.IsNull(parser.Parse("MA[20].["));
            Assert.IsNull(parser.Parse("MA[20].]"));
            Assert.IsNull(parser.Parse("MA[20].V("));
            Assert.IsNull(parser.Parse("MA[20].V)"));
            Assert.IsNull(parser.Parse("MA[20].V."));
            Assert.IsNull(parser.Parse("MA[20].V["));
            Assert.IsNull(parser.Parse("MA[20].V]"));


            // valid selection syntax
            Assert.IsNotNull(parser.Parse("MA[20].V"));
            Assert.IsNotNull(parser.Parse("MA[20].V "));
            Assert.IsNotNull(parser.Parse("MA[20]. V"));
            Assert.IsNotNull(parser.Parse("MA[20] .V"));
            Assert.IsNotNull(parser.Parse("MA[20](BAR).V"));
            Assert.IsNotNull(parser.Parse("MA[20](BAR.CP).V"));
        }
    }
}
