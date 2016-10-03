using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public static class StockBoardIndex
    {
        private const string GrowingBoardIndexCode = "SZ.399006";

        private const string SmallMiddleBoardIndexCode = "SZ.399005";

        private const string MainBoardIndexCode = "SZ.399300";

        public static StockName GrowingBoardIndexName = new StockName(GrowingBoardIndexCode, "创业板指数");
        public static StockName SmallMiddleBoardIndexName = new StockName(SmallMiddleBoardIndexCode, "中小板指数");
        public static StockName MainBoardIndexName = new StockName(MainBoardIndexCode, "沪深300指数");

        public static StockName GetBoardIndexName(StockBoard board)
        {
            switch(board)
            {
                case StockBoard.GrowingBoard:
                    return GrowingBoardIndexName;
                case StockBoard.SmallMiddleBoard:
                    return SmallMiddleBoardIndexName;
                case StockBoard.MainBoard:
                    return MainBoardIndexName;
                default:
                    return MainBoardIndexName;
            }
        }
    }
}
