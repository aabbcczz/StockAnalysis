﻿using System;
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

        ITradingObject GetTradingObject(string code);

        ITradingObject GetBoardIndexTradingObject(ITradingObject tradingObject);

        ITradingObject GetBoardIndexTradingObject(StockBoard board);

        Bar GetBarOfTradingObjectForCurrentPeriod(ITradingObject tradingObject);

        void Log(string log);

        void DumpBarsFromCurrentPeriod(ITradingObject tradingObject);

        void SetDefaultPriceForInstructionWhenNecessary(Instruction instruction);

        int GetPositionFrozenDays();
    }
}
