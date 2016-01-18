using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    sealed class QueryGeneralOrderResult
    {
        private static string[] columns = new string[]
        {
            "委托编号",
            "委托时间",
            "证券代码",
            "证券名称",
            "买卖标志",
            "状态说明",
            "委托价格",
            "委托数量",
            "成交价格",
            "成交数量",
            "委托方式",
            "报价方式",
        };

        private static int[] columnIndices = null;

        /// <summary>
        /// 委托编号
        /// </summary>
        public int OrderNo { get; private set; }

        /// <summary>
        /// 委托时间
        /// </summary>
        public string SubmissionTime { get; private set; }

        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecurityCode { get; private set; }

        /// <summary>
        /// 证券名称
        /// </summary>
        public string SecurityName { get; private set; }

        /// <summary>
        /// 买卖标志
        /// </summary>
        public string BuySellFlag { get; private set; }

        /// <summary>
        /// 状态说明
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public float SubmissionPrice { get; private set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public float SubmissionVolume { get; private set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public float DealPrice { get; private set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public float DealVolume { get; private set; }

        /// <summary>
        /// 委托方式
        /// </summary>
        public string SubmissionType { get; private set; }

        /// <summary>
        /// 报价方式
        /// </summary>
        public string PricingType { get; private set; }

        public static IEnumerable<QueryGeneralOrderResult> ExtractFrom(TabulateData data)
        {
            if (columnIndices == null)
            {
                columnIndices = columns.Select(c => data.GetColumnIndex(c)).ToArray();
            }

            var subData = data.GetSubColumns(columnIndices);

            foreach (var row in subData.Rows)
            {
                QueryGeneralOrderResult result = new QueryGeneralOrderResult();

                int index = 0;
                result.OrderNo = int.Parse(row[index++]);
                result.SubmissionTime = row[index++];
                result.SecurityCode = row[index++];
                result.SecurityName = row[index++];
                result.BuySellFlag = row[index++];
                result.Status = row[index++];
                result.SubmissionPrice = TradingHelper.SafeParseFloat(row[index++]);
                result.SubmissionVolume = TradingHelper.SafeParseFloat(row[index++]);
                result.DealPrice = TradingHelper.SafeParseFloat(row[index++]);
                result.DealVolume = TradingHelper.SafeParseFloat(row[index++]);
                result.SubmissionType = row[index++];
                result.PricingType = row[index++];

                yield return result;
            }
        }
    }
}
