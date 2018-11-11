using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class QueryGeneralOrderResult
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
        /// 状态说明
        /// </summary>
        public string StatusString { get; private set; }

        /// <summary>
        /// 状态
        /// </summary>
        public OrderStatus Status { get; private set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public float SubmissionPrice { get; private set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public int SubmissionVolume { get; private set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public float DealPrice { get; private set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int DealVolume { get; private set; }

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
                columnIndices = data.GetColumnIndices(columns).ToArray();
            }

            var subData = data.GetSubColumns(columnIndices);

            foreach (var row in subData.Rows)
            {
                QueryGeneralOrderResult result = new QueryGeneralOrderResult();

                int index = 0;
                result.OrderNo = int.Parse(row[index++]);
                result.SubmissionTime = row[index++];
                result.SecuritySymbol = row[index++];
                result.SecurityName = row[index++];
                result.BuySellFlag = row[index++];
                result.StatusString = row[index++];
                result.Status = TradingHelper.ConvertStringToOrderStatus(result.StatusString);

                result.SubmissionPrice = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.SubmissionVolume = TradingHelper.SafeParseInt(row[index++]);
                result.DealPrice = TradingHelper.SafeParseFloat(row[index++], 0.0f);
                result.DealVolume = TradingHelper.SafeParseInt(row[index++]);
                result.SubmissionType = row[index++];
                result.PricingType = row[index++];

                yield return result;
            }
        }
    }
}
