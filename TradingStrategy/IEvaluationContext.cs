using System;
using System.Collections.Generic;

namespace TradingStrategy
{
    public interface IEvaluationContext
    {
        double GetInitialEquity();

        double GetCurrentEquity(DateTime period, EquityEvaluationMethod method);

        IEnumerable<string> GetAllPositionCodes();

        bool ExistsPosition(string code);

        IEnumerable<Position> GetPositionDetails(string code);

        IEnumerable<ITradingObject> GetAllTradingObjects();

        int GetCountOfTradingObjects();

        void Log(string log);
    }
}
