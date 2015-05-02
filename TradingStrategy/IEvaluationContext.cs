using System;
using System.Collections.Generic;

using StockAnalysis.Share;

namespace TradingStrategy
{
    public interface IEvaluationContext
    {
        IRuntimeMetricManager MetricManager { get; }

        IGroupRuntimeMetricManager GroupMetricManager { get; }

        /// <summary>
        /// stock block relationship manager, could be null.
        /// </summary>
        StockBlockRelationshipManager RelationshipManager { get; }

        double GetInitialEquity();

        double GetCurrentEquity(DateTime period, EquityEvaluationMethod method);

        IEnumerable<string> GetAllPositionCodes();

        bool ExistsPosition(string code);

        IEnumerable<Position> GetPositionDetails(string code);

        IEnumerable<ITradingObject> GetAllTradingObjects();

        int GetCountOfTradingObjects();

        Bar GetBarOfTradingObjectForCurrentPeriod(ITradingObject tradingObject);

        void Log(string log);

        void DumpBarsFromCurrentPeriod(ITradingObject tradingObject);
    }
}
