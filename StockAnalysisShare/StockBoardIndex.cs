using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public static class StockBoardIndex
    {
        public const string GrowingBoardIndex = "399006";

        public const string SmallMiddleBoardIndex = "399005";

        public const string MainBoardIndex = "399300";

        public static string GetBoardIndex(StockBoard board)
        {
            switch(board)
            {
                case StockBoard.GrowingBoard:
                    return GrowingBoardIndex;
                case StockBoard.SmallMiddleBoard:
                    return SmallMiddleBoardIndex;
                case StockBoard.MainBoard:
                    return MainBoardIndex;
                default:
                    return MainBoardIndex;
            }
        }
    }
}
