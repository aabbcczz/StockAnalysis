using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    [Serializable]
    public sealed class CombinedStrategyGlobalSettings
    {
        public int MaxNumberOfActiveStocks { get; set; }

        public int MaxNumberOfActiveStocksPerBlock { get; set; }

        public OpenPositionInstructionOrder InstructionOder { get; set; }

        public InstructionSortMode InceasePositionInstructionSortMode { get; set; }

        public InstructionSortMode NewPositionInstructionSortMode { get; set; }

        public bool IncreaseStoplossPriceEvenIfTransactionFailed { get; set; }

        public static CombinedStrategyGlobalSettings GenerateExsampleSettings()
        {
            return new CombinedStrategyGlobalSettings()
            {
                MaxNumberOfActiveStocks = 1000,
                MaxNumberOfActiveStocksPerBlock = 1000,
                InstructionOder = OpenPositionInstructionOrder.IncPosThenNewPos,
                InceasePositionInstructionSortMode = InstructionSortMode.NoSorting,
                NewPositionInstructionSortMode = InstructionSortMode.NoSorting,
                IncreaseStoplossPriceEvenIfTransactionFailed = false,
            };
        }
    }
}
