using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class CommonPositionAdjusting : GeneralPositionAdjustingBase
    {
        public override string Name
        {
            get { return "通用头寸调整策略"; }
        }

        public override string Description
        {
            get { return "按照参数规定当盈利达到一定程度就加仓，直到最大仓位。"; }
        }

        [Parameter(4, "单交易对象最大允许的头寸个数（非总头寸数）")]
        public int MaxPositionCountOfEachObject { get; set; }

        [Parameter("0.5:0.5", 
@"加仓规则，由若干组浮点数表示，例如0.5:0.5:1.0:0.5包含两组数（0.5,0.5）和（1.0,0.5）。
每组数（x,y）表示的规则是：当第一个头寸的利润")]
        public string PositionIncreaseRule { get; set; }

        public override IEnumerable<Instruction> AdjustPositions()
        {
            throw new NotImplementedException();
        }
    }
}
