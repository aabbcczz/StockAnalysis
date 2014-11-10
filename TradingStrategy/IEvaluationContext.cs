using System;
using System.Collections.Generic;

using StockAnalysis.Share;

namespace TradingStrategy
{
    public interface IEvaluationContext
    {
        IRuntimeMetricManager MetricManager { get; }

        double GetInitialEquity();

        double GetCurrentEquity(DateTime period, EquityEvaluationMethod method);

        IEnumerable<string> GetAllPositionCodes();

        bool ExistsPosition(string code);

        IEnumerable<Position> GetPositionDetails(string code);

        IEnumerable<ITradingObject> GetAllTradingObjects();

        int GetCountOfTradingObjects();

        Bar GetBarOfTradingObjectForCurrentPeriod(ITradingObject tradingObject);

        void Log(string log);
    }
}
