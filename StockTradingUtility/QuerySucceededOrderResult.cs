using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class QuerySucceededOrderResult
    {
        private static string[] columns = new string[]
        {
            "委托编号",
            "成交编号",
            "成交时间",
            "证券代码",
            "证券名称",
            "买卖标志",
            "成交价格",
            "成交数量",
            "成交金额",
        };

        private static int[] columnIndices = null;

        /// <summary>
        /// 委托编号
        /// </summary>
        public int OrderNo { get; private set; }

        /// <summary>
        /// 成交编号
        /// </summary>
        public int DealNo { get; private set; }

        /// <summary>
        /// 成交时间
        /// </summary>
        public string DealTime { get; private set;}
        
        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecuritySymbol { get; private set; }

        /// <summary>
        /// 证券名称
        /// </summary>
        public string SecurityName { get; private set; }

        /// <summary>
        /// 买卖标志
        /// </summary>
        public string BuySellFlag { get; private set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public float DealPrice { get; private set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public float DealVolume { get; private set; }

        /// <summary>
        /// 成交金额
        /// </summary>
        public float DealAmount { get; private set; }


        public static IEnumerable<QuerySucceededOrderResult> ExtractFrom(TabulateData data)
        {
            if (columnIndices == null)
            {
                columnIndices = data.GetColumnIndices(columns).ToArray();
            }

            var subData = data.GetSubColumns(columnIndices);

            foreach (var row in subData.Rows)
            {
                QuerySucceededOrderResult result = new QuerySucceededOrderResult();

                int index = 0;
                result.OrderNo = int.Parse(row[index++]);
                result.DealNo = int.Parse(row[index++]);
                result.DealTime = row[index++];
                result.SecuritySymbol = row[index++];
                result.SecurityName = row[index++];
                result.BuySellFlag = row[index++];
                result.DealPrice = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.DealVolume = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.DealAmount = TradingHelper.SafeParseFloat(row[index++], 0.0f);

                yield return result;
            }
        }
    }
}