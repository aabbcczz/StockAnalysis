using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TradingStrategy.Base
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

        [Parameter(OpenPositionInstructionOrder.IncPosThenNewPos, "指令排序值，为0/IncPosThenNewPos时表示加仓指令排在新建仓指令之前，为1/NewPosThenIncPos时表示新建仓指令拍在加仓指令之前")]
        public OpenPositionInstructionOrder InstructionOrder { get; set; }

        [Parameter(InstructionSortMode.NoSorting, @"加仓指令排序模式，取值为：
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
        public InstructionSortMode IncreasePositionInstructionSortMode { get; set; }

        // metric used for sort instruction for increasing position. it is used only when 
        // InceasePositionInstructionSortMode is SortByMetricAscending or SortByMetricDescending
        [Parameter("ATR[20]", "加仓指令排序指标，当且仅当加仓指令排序模式为8/9时有效")]
        public string IncreasePositionSortMetric { get; set; }

        [Parameter(InstructionSortMode.NoSorting, @"新建仓指令排序模式，取值为：
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
        public InstructionSortMode NewPositionInstructionSortMode { get; set; }

        // metric used for sorting instruction for creating new position. it is used only when 
        // NewPositionInstructionSortMode is SortByMetricAscending or SortByMetricDescending
        [Parameter("ATR[20]", "新建仓指令排序指标，当且仅当新建仓指令排序模式为8/9时有效")]
        public string NewPositionSortMetric { get; set; }

        // value used to control the random seeds when the sort mode is Randomize. 
        [Parameter(0, "Random seeds")]
        public int RandomSeeds { get; set; }

        [Parameter(false, "Randomly remove instruction")]
        public bool RandomlyRemoveInstruction { get; set; }

        [Parameter(50, "Randomly remove instruction threshold. [0..99]")]
        public int RandomlyRemoveInstructionThreshold { get; set; }

        [Parameter(false, "优先执行卖出指令以获得资金")]
        public bool CloseInstructionFirst { get; set; }

        [Parameter(false, "只允许在价格上涨时出发入市信号")]
        public bool AllowEnteringMarketOnlyWhenPriceIncreasing { get; set; }

        [Parameter(false, "用收盘价判断止损")]
        public bool StopLossByClosePrice { get; set; }

        [Parameter(1.0, "真实买入价比例")]
        public double TrueBuyPriceScale { get; set; }

        public RuntimeMetricProxy IncreasePositionSortMetricProxy { get; private set; }
        public RuntimeMetricProxy NewPositionSortMetricProxy { get; private set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            IncreasePositionSortMetricProxy = null;
            NewPositionSortMetricProxy = null;

            // register metric for sorting instruction
            if (!string.IsNullOrWhiteSpace(IncreasePositionSortMetric))
            {
                IncreasePositionSortMetricProxy = new RuntimeMetricProxy(Context.MetricManager, IncreasePositionSortMetric);
            }

            if (!string.IsNullOrWhiteSpace(NewPositionSortMetric))
            {
                NewPositionSortMetricProxy = new RuntimeMetricProxy(Context.MetricManager, NewPositionSortMetric);
            }
        }
    }
}
