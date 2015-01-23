using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TradingStrategy.Strategy
{
    public sealed class GlobalSettingsComponent : GeneralTradingStrategyComponentBase
    {
        public override string Name
        {
            get { return "全局设定模块"; }
        }

        public override string Description
        {
            get { return "虚拟模块，用于设定全局参数"; }
        }

        [Parameter(1000, "最大允许持仓股票个数")]
        public int MaxNumberOfActiveStocks { get; set; }

        [Parameter(1000, "最大允许每个板块持仓股票个数")]
        public int MaxNumberOfActiveStocksPerBlock { get; set; }

        public OpenPositionInstructionOrder InstructionOrder 
        { 
            get { return (OpenPositionInstructionOrder)InstructionOrderValue; } 
        }

        [Parameter(0, "指令排序值，为0时表示加仓指令排在新建仓指令之前，为1时表示新建仓指令拍在加仓指令之前")]
        public int InstructionOrderValue {get; set;}

        public InstructionSortMode InceasePositionInstructionSortMode 
        { 
            get { return (InstructionSortMode)IncreasePositionInstructionSortModeValue; } 
        }

        [Parameter(4, @"加仓指令排序模式，取值为：
NoSorting = 0,
Randomize = 1,
SortByInstructionIdAscending = 2,
SortByInstructionIdDescending = 3,
SortByCodeAscending = 4,
SortByCodeDescending = 5,
SortByVolumeAscending = 6,
SortByVolumeDescending = 7,
SortByMetricAscending = 8,
SortByMetricDescending = 9")]
        public int IncreasePositionInstructionSortModeValue { get; set; }

        // metric used for sort instruction for increasing position. it is used only when 
        // InceasePositionInstructionSortMode is SortByMetricAscending or SortByMetricDescending
        [Parameter("ATR[20]", "加仓指令排序指标，当且仅当加仓指令排序模式为8/9时有效")]
        public string IncreasePositionSortMetric { get; set; }

        public InstructionSortMode NewPositionInstructionSortMode 
        {
            get { return (InstructionSortMode)NewPositionInstructionSortModeValue; }
        }

        [Parameter(4, @"新建仓指令排序模式，取值为：
NoSorting = 0,
Randomize = 1,
SortByInstructionIdAscending = 2,
SortByInstructionIdDescending = 3,
SortByCodeAscending = 4,
SortByCodeDescending = 5,
SortByVolumeAscending = 6,
SortByVolumeDescending = 7,
SortByMetricAscending = 8,
SortByMetricDescending = 9")]
        public int NewPositionInstructionSortModeValue { get; set; }

        // metric used for sorting instruction for creating new position. it is used only when 
        // NewPositionInstructionSortMode is SortByMetricAscending or SortByMetricDescending
        [Parameter("ATR[20]", "新建仓指令排序指标，当且仅当新建仓指令排序模式为8/9时有效")]
        public string NewPositionSortMetric { get; set; }

        public bool IncreaseStoplossPriceEvenIfTransactionFailed 
        {
            get { return IncreaseStoplossPriceEvenIfTransactionFailedValue != 0; }
        }

        // value used to control the random seeds when the sort mode is Randomize. 
        [Parameter(0, "Random seeds")]
        public int RandomSeeds { get; set; }

        [Parameter(0, "是否当加仓指令无法执行时依旧按照加仓成功来提高止损位. 0 表示 false, 1 表示 true")]
        public int IncreaseStoplossPriceEvenIfTransactionFailedValue { get; set; }


        public int IncreasePositionSortMetricIndex { get; private set; }
        public int NewPositionSortMetricIndex { get; private set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            IncreasePositionSortMetricIndex = -1;
            NewPositionSortMetricIndex = -1;

            // register metric for sorting instruction
            if (!string.IsNullOrWhiteSpace(IncreasePositionSortMetric))
            {
                IncreasePositionSortMetricIndex = Context.MetricManager.RegisterMetric(IncreasePositionSortMetric);
            }

            if (!string.IsNullOrWhiteSpace(NewPositionSortMetric))
            {
                NewPositionSortMetricIndex = Context.MetricManager.RegisterMetric(NewPositionSortMetric);
            }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (MaxNumberOfActiveStocks <= 0)
            {
                throw new ArgumentOutOfRangeException("MaxNumberOfActiveStocks must be greater than 0");
            }

            if (MaxNumberOfActiveStocksPerBlock <= 0)
            {
                throw new ArgumentOutOfRangeException("MaxNumberOfActiveStocksPerBlock must be greater than 0");
            }

            if (!Enum.IsDefined(typeof(OpenPositionInstructionOrder), InstructionOrderValue))
            {
                throw new ArgumentException("InstructionOrderValue is invalid");
            }

            if (!Enum.IsDefined(typeof(InstructionSortMode), IncreasePositionInstructionSortModeValue))
            {
                throw new ArgumentException("IncreasePositionInstructionSortModeValue is invalid");
            }

            if (!Enum.IsDefined(typeof(InstructionSortMode), NewPositionInstructionSortModeValue))
            {
                throw new ArgumentException("NewPositionInstructionSortModeValue is invalid");
            }

            if (IncreaseStoplossPriceEvenIfTransactionFailedValue != 0 && IncreaseStoplossPriceEvenIfTransactionFailedValue != 1)
            {
                throw new ArgumentException("IncreaseStoplossPriceEvenIfTransactionFailedValue is invalid");
            }
        }
    }
}
