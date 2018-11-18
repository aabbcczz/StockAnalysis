namespace StockAnalysis.StockTrading.Utility
{
    public interface ITradingServer
    {
        /// <summary>
        /// 交易账户登录
        /// </summary>
        /// <param name="IP">券商交易服务器IP</param>
        /// <param name="port">券商交易服务器端口</param>
        /// <param name="version">设置通达信客户端的版本号</param>
        /// <param name="yybId">营业部代码，请到网址 http://www.chaoguwaigua.com/downloads/qszl.htm 查询</param>
        /// <param name="accountNo">完整的登录账号，券商一般使用资金帐户或客户号</param>
        /// <param name="tradeAccount">交易账号，一般与登录帐号相同. 请登录券商通达信软件，查询股东列表，股东列表内的资金帐号就是交易帐号, 具体查询方法请见网站“热点问答”栏目</param>
        /// <param name="tradePassword">交易密码</param>
        /// <param name="communicationPassword">通讯密码</param>
        /// <param name="error">此API执行返回后，如果出错，保存了错误信息说明。一般要分配256字节的空间。没出错时为空字符串。</param>
        /// <returns>客户端ID，失败时返回-1</returns>
        int Logon(string IP, short port, string version, short yybId, string accountNo, string tradeAccount, string tradePassword, string communicationPassword, out string error);

        /// <summary>
        /// 交易账户注销
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        void Logoff(int clientId);

        /// <summary>
        /// 查询各种交易数据
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="category">表示查询信息的种类，0资金  1股份   2当日委托  3当日成交     4可撤单   5股东代码  6融资余额   7融券余额  8可融证券</param>
        /// <param name="result">此API执行返回后，Result内保存了返回的查询数据, 形式为表格数据，行数据之间通过\n字符分割，列数据之间通过\t分隔。一般要分配1024*1024字节的空间。出错时为空字符串。</param>
        /// <param name="error">同Logon函数的ErrInfo说明</param>
        void QueryData(int clientId, int category, out string result, out string error);

        /// <summary>
        /// 下委托交易证券
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="category">表示委托的种类，0买入 1卖出  2融资买入  3融券卖出   4买券还券   5卖券还款  6现券还券</param>
        /// <param name="priceType">表示报价方式 0上海限价委托 深圳限价委托 1(市价委托)深圳对方最优价格  2(市价委托)深圳本方最优价格  3(市价委托)深圳即时成交剩余撤销  4(市价委托)上海五档即成剩撤 深圳五档即成剩撤 5(市价委托)深圳全额成交或撤销 6(市价委托)上海五档即成转限价
        /// <param name="shareholderCode">股东代码, 交易上海股票填上海的股东代码；交易深圳的股票填入深圳的股东代码</param>
        /// <param name="securitySymbol">证券代码</param>
        /// <param name="price">委托价格</param>
        /// <param name="quantity">委托数量</param>
        /// <param name="result">同上,其中含有委托编号数据</param>
        /// <param name="error">同上</param>
        void SendOrder(int clientId, int category, int priceType, string shareholderCode, string securitySymbol, float price, int quantity, out string result, out string error);

        /// <summary>
        /// 撤委托
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="exchangeId">交易所ID， 上海1，深圳0(招商证券普通账户深圳是2)</param>
        /// <param name="orderNo">表示要撤的目标委托的编号</param>
        /// <param name="result">同上</param>
        /// <param name="error">同上</param>
        void CancelOrder(int clientId, string exchangeId, string orderNo, out string result, out string error);

        /// <summary>
        /// 获取证券的实时五档行情
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="securitySymbol">证券代码</param>
        /// <param name="result">同上</param>
        /// <param name="error">同上</param>
        void GetQuote(int clientId, string securitySymbol, out string result, out string error);

        ///普通批量版
        /// <summary>
        /// 属于普通批量版功能,批量查询各种交易数据,用数组传入每个委托的参数，数组第i个元素表示第i个查询的相应参数
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="categories">信息的种类的数组, 第i个元素表示第i个查询的信息种类，0资金  1股份   2当日委托  3当日成交     4可撤单   5股东代码  6融资余额   7融券余额  8可融证券</param>
        /// <param name="categoryCount">查询的个数，即数组的长度</param>
        /// <param name="results">返回数据的数组, 第i个元素表示第i个委托的返回信息. 此API执行返回后，Result[i]含义同上。</param>
        /// <param name="errors">错误信息的数组，第i个元素表示第i个委托的错误信息. 此API执行返回后，ErrInfo[i]含义同上。</param>
        void QueryData(int clientId,
            int[] categories,
            int categoryCount,
            out string[] results,
            out string[] errors);

        /// <summary>
        /// 属于普通批量版功能,批量下委托交易证券，用数组传入每个委托的参数，数组第i个元素表示第i个委托的相应参数
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="categories">委托种类的数组，第i个元素表示第i个委托的种类，0买入 1卖出  2融资买入  3融券卖出   4买券还券   5卖券还款  6现券还券</param>
        /// <param name="priceTypes">表示报价方式的数组,  第i个元素表示第i个委托的报价方式, 0上海限价委托 深圳限价委托 1(市价委托)深圳对方最优价格  2(市价委托)深圳本方最优价格  3(市价委托)深圳即时成交剩余撤销  4(市价委托)上海五档即成剩撤 深圳五档即成剩撤 5(市价委托)深圳全额成交或撤销 6(市价委托)上海五档即成转限价
        /// <param name="shareholderCodes">股东代码数组，第i个元素表示第i个委托的股东代码，交易上海股票填上海的股东代码；交易深圳的股票填入深圳的股东代码</param>
        /// <param name="securitySymbols">证券代码数组，第i个元素表示第i个委托的证券代码</param>
        /// <param name="prices">委托价格数组，第i个元素表示第i个委托的委托价格</param>
        /// <param name="quantities">委托数量数组，第i个元素表示第i个委托的委托数量</param>
        /// <param name="orderCount">委托的个数，即数组的长度</param>
        /// <param name="results">同上</param>
        /// <param name="errors">同上</param>
        void SendOrders(int clientId, int[] categories, int[] priceTypes, string[] shareholderCodes, string[] securitySymbols, float[] prices, int[] quantities, int orderCount, out string[] results, out string[] errors);

        /// <summary>
        /// 属于普通批量版功能,批量撤委托, 用数组传入每个委托的参数，数组第i个元素表示第i个撤委托的相应参数
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="exchangeIds">交易所ID， 上海1，深圳0(招商证券普通账户深圳是2)</param>
        /// <param name="orderNoes">表示要撤的目标委托的编号</param>
        /// <param name="orderCount">撤委托的个数，即数组的长度</param>
        /// <param name="results">同上</param>
        /// <param name="errors">同上</param>
        void CancelOrders(int clientId, string[] exchangeIds, string[] orderNoes, int orderCount, out string[] results, out string[] errors);

        /// <summary>
        /// 属于普通批量版功能,批量获取证券的实时五档行情
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="securitySymbols">证券代码</param>
        /// <param name="result">同上</param>
        /// <param name="errors">同上</param>
        void GetQuotes(int clientId, string[] securitySymbols, int securityCount, out string[] results, out string[] errors);

        /// <summary>
        /// 属于普通批量版功能,查询各种历史数据
        /// </summary>
        /// <param name="clientId">客户端ID</param>
        /// <param name="category">表示查询信息的种类，0历史委托  1历史成交   2交割单</param>
        /// <param name="startDate">表示开始日期，格式为yyyyMMdd,比如2014年3月1日为  20140301
        /// <param name="endDate">表示结束日期，格式为yyyyMMdd,比如2014年3月1日为  20140301
        /// <param name="result">同上</param>
        /// <param name="error">同上</param>
        void QueryHistoryData(int clientId, int category, string startDate, string endDate, out string result, out string error);
    }

}
