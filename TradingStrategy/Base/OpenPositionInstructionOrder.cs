namespace StockAnalysis.TradingStrategy.Base
{
    public enum OpenPositionInstructionOrder
    {
        IncPosThenNewPos = 0, // instructions for increasing positions are put before instructions for new position
        NewPosThenIncPos = 1 // instructions for increasing positions are put behind instructions for new position
    }
}
