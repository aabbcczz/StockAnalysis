using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class SendOrderResult
    {
        private static string[] columns = new string[]
        {
            "委托编号",
            "返回信息",
            "检查风险标志",
            "保留信息"
        };

        private static int[] columnIndices = null;

        /// <summary>
        /// 委托编号
        /// </summary>
        public int OrderNo { get; private set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string ReturnedInfo { get; private set; }

        /// <summary>
        /// 检查风险标志
        /// </summary>
        public string CheckingRiskFlag { get; private set; }

        /// <summary>
        /// 保留信息
        /// </summary>
        public string ReservedInfo { get; private set; }


        public static IEnumerable<SendOrderResult> ExtractFrom(TabulateData data)
        {
            if (columnIndices == null)
            {
                columnIndices = columns.Select(c => data.GetColumnIndex(c)).ToArray();
            }

            var subData = data.GetSubColumns(columnIndices);

            foreach (var row in subData.Rows)
            {
                SendOrderResult result = new SendOrderResult();

                int index = 0;
                result.OrderNo = int.Parse(row[index++]);
                result.ReturnedInfo = row[index++];
                result.CheckingRiskFlag = row[index++];
                result.ReservedInfo = row[index++];

                yield return result;
            }
        }
    }
}
