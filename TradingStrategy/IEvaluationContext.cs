using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public interface IEvaluationContext
    {
        double GetCurrentCapital();

        IEnumerable<string> GetAllPositionCodes();

        bool ExistsPosition(string code);

        IEnumerable<Position> GetPositionDetails(string code);

        void Log(string log);
    }
}
