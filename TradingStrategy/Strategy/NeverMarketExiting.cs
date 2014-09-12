using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class NeverMarketExiting 
        : GeneralTradingStrategyComponentBase
        , IMarketExitingComponent
    {
        public override string Name
        {
            get { return "从不退市"; }
        }

        public override string Description
        {
            get { return "从不退市，依赖于初始止损或者其他模块退市"; }
        }

        public bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            return false;
        }
    }
}
