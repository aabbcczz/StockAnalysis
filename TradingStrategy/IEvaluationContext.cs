using System;
using System.Collections.Generic;

using StockAnalysis.Common.Data;
using StockAnalysis.Common.ChineseMarket;

using StockAnalysis.TradingStrategy.Base;

namespace StockAnalysis.TradingStrategy
{
    public interface IEvaluationContext
    {
        IRuntimeMetricManager MetricManager { get; }

        IGroupRuntimeMetricManager GroupMetricManager { get; }

        /// <summary>
        /// stock block relationship manager, could be null.
        /// </summary>
        StockBlockRelationshipManager RelationshipManager { get; }

        GlobalSettingsComponent GlobalSettings { get; set; }

        double GetInitialEquity();

        double GetCurrentEquity(DateTime period, EquityEvaluationMethod method);

        IEnumerable<string> GetAllPositionSymbols();

        bool ExistsPosition(string symbol);

        IEnumerable<Position> GetPositionDetails(string symbol);

        IEnumerable<ITradingObject> GetAllTradingObjects();

        int GetCountOfTradingObjects();

        ITradingObject GetTradingObject(string symbol);

        ITradingObject GetBoardIndexTradingObject(ITradingObject tradingObject);

        ITradingObject GetBoardIndexTradingObject(StockBoard board);

        Bar GetBarOfTradingObjectForCurrentPeriod(ITradingObject tradingObject);

        void Log(string log);

        void DumpBarsFromCurrentPeriod(ITradingObject tradingObject);

        void SetDefaultPriceForInstructionWhenNecessary(Instruction instruction);

        int GetPositionFrozenDays();
    }
}
