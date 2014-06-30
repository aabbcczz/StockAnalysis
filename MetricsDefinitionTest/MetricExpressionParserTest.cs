using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MetricsDefinition;

namespace MetricsDefinitionTest
{
    [TestClass]
    public class MetricExpressionParserTest
    {
        [TestMethod]
        public void TestParse()
        {
            MetricExpressionParser parser = new MetricExpressionParser();

            string errorMessage;

            try
            {
                // null input
                parser.Parse(null, out errorMessage);
            }
            catch (ArgumentNullException)
            {
                // pass
            }

            // empty input
            Assert.IsNull(parser.Parse("", out errorMessage));
            Assert.IsNull(parser.Parse("  \t\n", out errorMessage));

            // case sensitivity test
            Assert.IsNull(parser.Parse("ma[20]", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[20]", out errorMessage));

            // unknown metric name
            Assert.IsNull(parser.Parse("M", out errorMessage));

            // multiple metric name
            Assert.IsNotNull(parser.Parse("EMA[20]", out errorMessage));
            Assert.IsNotNull(parser.Parse("EXPMA[20]", out errorMessage));

            // invalid parameters
            Assert.IsNull(parser.Parse("MA", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20,10]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20;10]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20(10]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20)10]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[abc]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20.00]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[.00]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[-20]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[+20]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20a]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20.00a]", out errorMessage));

            // valid parameters
            Assert.IsNotNull(parser.Parse("MA[20]", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[20] ", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA [20]", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[ 20 ]", out errorMessage));

            // valid syntax
            Assert.IsNotNull(parser.Parse("S", out errorMessage));
            Assert.IsNotNull(parser.Parse("S[]", out errorMessage));

            // invalid embed syntax
            Assert.IsNull(parser.Parse("S()", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20]()", out errorMessage));
            Assert.IsNull(parser.Parse("S[", out errorMessage));
            Assert.IsNull(parser.Parse("S]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20", out errorMessage));
            Assert.IsNull(parser.Parse("S,", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20,", out errorMessage));

            Assert.IsNull(parser.Parse("S(S", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20](S", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20](MA[20]", out errorMessage));

            // valid embeded syntax
            Assert.IsNotNull(parser.Parse("S(S)", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[20](S)", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[20](MA[20])", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[20](MA[20](S))", out errorMessage));

            // invalid selection syntax
            Assert.IsNull(parser.Parse("MA[20].", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].X", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].1", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].(", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].)", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20]..", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].[", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].]", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].V(", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].V)", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].V.", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].V[", out errorMessage));
            Assert.IsNull(parser.Parse("MA[20].V]", out errorMessage));


            // valid selection syntax
            Assert.IsNotNull(parser.Parse("MA[20].V", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[20].V ", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[20]. V", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[20] .V", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[20](S).V", out errorMessage));
            Assert.IsNotNull(parser.Parse("MA[20](S.CP).V", out errorMessage));
        }
    }
}
