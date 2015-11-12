namespace TradingStrategy
{
    public enum EquityEvaluationMethod
    {
        CoreEquity = 0, // cash as equity
        TotalEquity = 1, // cash + the market values of all positions
        ReducedTotalEquity = 2, // cash + the market values of all positions - the risks of all positions
        InitialEquity = 3, // initial cash as equity
        LossControlInitialEquity = 4, // when the total equity > initial equity, initial equity will be returned. 
                                      // Otherwise, initial equity - 2 * (initial equity - total equity) will be returned.
        LossControlTotalEquity = 5, // when the total equity > initial equity, total equity will be returned. 
                                    // Otherwise, initial equity - 2 * (initial equity - total equity) will be returned.
        LossControlReducedTotalEquity = 6, // when the reduced total equity > initial equity, reduced total equity will be returned. 
                                            // Otherwise, initial equity - 2 * (initial equity - reduced total equity) will be returned.
    }
}
