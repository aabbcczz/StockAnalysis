using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTrading
{
    sealed class QueryShareholderRegistryResult
    {
        private static string[] columns = new string[]
        {
            "股东代码",
            "帐号类别",
            "资金帐号",
            "席位代码",
            "保留信息"
        };

        private static int[] columnIndices = null;

        /// <summary>
        /// 股东代码
        /// </summary>
        public string ShareholderCode { get; private set; }

        /// <summary>
        /// 所属交易所
        /// </summary>
        public Exchange Exchange { get; private set; }

        /// <summary>
        /// 资金账号
        /// </summary>
        public string CapitalAccount { get; private set; }

        /// <summary>
        /// 席位代码
        /// </summary>
        public string SeatCode { get; private set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Notes { get; private set; }

        public static IEnumerable<QueryShareholderRegistryResult> ExtractFrom(TabulateData data)
        {
            if (columnIndices == null)
            {
                columnIndices = columns.Select(c => data.GetColumnIndex(c)).ToArray();
            }

            var subData = data.GetSubColumns(columnIndices);

            foreach (var row in subData.Rows)
            {
                yield return new QueryShareholderRegistryResult()
                {
                    ShareholderCode = row[0],
                    Exchange = row[1] == "0" ? Exchange.ShenzhenExchange : (row[1] == "1" ? Exchange.ShanghaiExchange : null),
                    CapitalAccount = row[2],
                    SeatCode = row[3],
                    Notes = row[4],
                };
            }
        }
    }
}
