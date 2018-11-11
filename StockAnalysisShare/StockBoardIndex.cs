using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public static class StockBoardIndex
    {
        private const string GrowingBoardIndexSymbol = "SZ.399006";

        private const string SmallMiddleBoardIndexSymbol = "SZ.399005";

        private const string MainBoardIndexSymbol = "SZ.399300";

        public static StockName GrowingBoardIndexName = new StockName(GrowingBoardIndexSymbol, "创业板指数");
        public static StockName SmallMiddleBoardIndexName = new StockName(SmallMiddleBoardIndexSymbol, "中小板指数");
        public static StockName MainBoardIndexName = new StockName(MainBoardIndexSymbol, "沪深300指数");

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
