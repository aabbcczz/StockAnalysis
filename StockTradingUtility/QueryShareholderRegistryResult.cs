using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalysis.Share;

namespace StockTrading.Utility
{
    public sealed class QueryShareholderRegistryResult
    {
        private static string[] columns = new string[]
        {
            "股东代码",
            "股东名称|股东姓名",
            "帐号类别",
            "资金帐号",
            "席位代码|交易席位",
            "融资融券标识",
            "保留信息"
        };

        private static int[] columnIndices = null;

        /// <summary>
        /// 股东代码
        /// </summary>
        public string ShareholderCode { get; private set; }

        /// <summary>
        /// 股东名称
        /// </summary>
        public string ShareholderName { get; private set; }

        /// <summary>
        /// 所属交易所
        /// </summary>
        public IExchange Exchange { get; private set; }

        /// <summary>
        /// 资金账号
        /// </summary>
        public string CapitalAccount { get; private set; }

        /// <summary>
        /// 席位代码
        /// </summary>
        public string SeatCode { get; private set; }

        /// <summary>
        /// 融资融券标识
        /// </summary>
        public bool IsCreditAccount { get; private set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Notes { get; private set; }

        public static IEnumerable<QueryShareholderRegistryResult> ExtractFrom(TabulateData data)
        {
            if (columnIndices == null)
            {
                columnIndices = data.GetColumnIndices(columns).ToArray();
            }

            var subData = data.GetSubColumns(columnIndices);

            foreach (var row in subData.Rows)
            {
                QueryShareholderRegistryResult result = new QueryShareholderRegistryResult();

                int index = 0;
                result.ShareholderCode = row[index++];
                result.ShareholderName = row[index++];
                result.Exchange = 
                    row[index] == "0" 
                    ? ExchangeFactory.GetExchangeById(ExchangeId.ShenzhenSecurityExchange) 
                    : (row[index] == "1" ? ExchangeFactory.GetExchangeById(ExchangeId.ShanghaiSecurityExchange) : null); 
                index++;

                result.CapitalAccount = row[index++];
                result.SeatCode = row[index++];

                result.IsCreditAccount = row[index] == "1";
                index++;

                result.Notes = row[index++];

                yield return result;
            }
        }
    }
}
