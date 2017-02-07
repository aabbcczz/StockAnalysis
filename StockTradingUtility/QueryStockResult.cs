using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class QueryStockResult
    {
        private static string[] columns = new string[]
        {
            "证券代码",
            "证券名称",
            "证券数量",
            "可卖数量",
            "参考成本价",
            "当前价",
            "最新市值",
            "盈亏比例(%)",
        };

        private static int[] columnIndices = null;

        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecurityCode { get; private set; }

        /// <summary>
        /// 证券名称
        /// </summary>
        public string SecurityName { get; private set; }

        /// <summary>
        /// 证券数量
        /// </summary>
        public float Volume { get; private set; }

        /// <summary>
        /// 可卖数量
        /// </summary>
        public float SellableVolume { get; private set; }

        /// <summary>
        /// 参考成本价
        /// </summary>
        public float ReferenceCost { get; private set; }

        /// <summary>
        /// 当前价
        /// </summary>
        public float CurrentPrice { get; private set; }

        /// <summary>
        /// 最新市值
        /// </summary>
        public float LatestMarketValue { get; private set; }

        /// <summary>
        /// 盈亏比例(%)
        /// </summary>
        public float ProfitPercentage { get; private set; }

        public static IEnumerable<QueryStockResult> ExtractFrom(TabulateData data)
        {
            if (columnIndices == null)
            {
                columnIndices = data.GetColumnIndices(columns).ToArray();
            }

            var subData = data.GetSubColumns(columnIndices);

            foreach (var row in subData.Rows)
            {
                QueryStockResult result = new QueryStockResult();

                int index = 0;

                result.SecurityCode = row[index++];
                result.SecurityName = row[index++];
                result.Volume = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.SellableVolume = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.ReferenceCost = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.CurrentPrice = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.LatestMarketValue = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.ProfitPercentage = TradingHelper.SafeParseFloat(row[index++], 0.0f);

                yield return result;
            }
        }
    }
}
