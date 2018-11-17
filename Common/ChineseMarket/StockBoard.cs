namespace StockAnalysis.Common.ChineseMarket
{
    using System;

    [Flags]
    public enum StockBoard
    {
        Unknown = 0,
        MainBoard = 0x01, // 主板
        SmallMiddleBoard = 0x02, // 中小板
        GrowingBoard = 0x04, // 创业板

        All = MainBoard | SmallMiddleBoard | GrowingBoard
    }
}
