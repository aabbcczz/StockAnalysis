namespace StockAnalysis.TradingStrategy.Base
{
    using System.Collections.Generic;

    public abstract class GeneralPositionAdjustingBase 
        : GeneralTradingStrategyComponentBase
        , IPositionAdjustingComponent
    {
        public abstract IEnumerable<Instruction> AdjustPositions();
    }
}
