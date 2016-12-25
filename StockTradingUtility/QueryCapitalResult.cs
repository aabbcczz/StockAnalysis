using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class QueryCapitalResult
    {
        private static string[] columns = new string[]
        {
            "资金余额",
            "可用资金",
            "冻结资金",
            "可取资金",
            "总资产",
        };

        private static int[] columnIndices = null;

        /// <summary>
        /// 资金余额
        /// </summary>
        public float RemainingCapital { get; private set; }

        /// <summary>
        /// 可用资金
        /// </summary>
        public float UsableCapital { get; private set; }

        /// <summary>
        /// 冻结资金
        /// </summary>
        public float FrozenCapital { get; private set; }

        /// <summary>
        /// 可取资金
        /// </summary>
        public float CashableCapital { get; private set; }

        /// <summary>
        /// 总资产
        /// </summary>
        public float TotalEquity { get; private set; }

        public static IEnumerable<QueryCapitalResult> ExtractFrom(TabulateData data)
        {
            if (columnIndices == null)
            {
                columnIndices = data.GetColumnIndices(columns).ToArray();
            }

            var subData = data.GetSubColumns(columnIndices);

            foreach (var row in subData.Rows)
            {
                QueryCapitalResult result = new QueryCapitalResult();

                int index = 0;
                result.RemainingCapital = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.UsableCapital = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.FrozenCapital = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.CashableCapital = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.TotalEquity = TradingHelper.SafeParseFloat(row[index++], 0.0f);

                yield return result;
            }
        }
    }
}
