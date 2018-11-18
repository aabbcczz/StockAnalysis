using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace StockAnalysis.StockTrading.Utility
{
    
//1.交易API均是Trade.dll文件的导出函数，包括以下函数：
//基本版的9个函数：
// void  OpenTdx();//打开通达信
// void  CloseTdx();//关闭通达信
//  int  Logon(char* IP, short Port, char* Version, short YybID, char* AccountNo,char* TradeAccount, char* JyPassword,   char* TxPassword, char* ErrInfo);//登录帐号
// void  Logoff(int ClientID);//注销
// void  QueryData(int ClientID, int Category, char* Result, char* ErrInfo);//查询各类交易数据
// void  SendOrder(int ClientID, int Category ,int PriceType,  char* Gddm,  char* Zqdm , float Price, int Quantity,  char* Result, char* ErrInfo);//下单
// void  CancelOrder(int ClientID, char* ExchangeID, char* hth, char* Result, char* ErrInfo);//撤单
// void  GetQuote(int ClientID, char* Zqdm, char* Result, char* ErrInfo);//获取五档报价
// void  Repay(int ClientID, char* Amount, char* Result, char* ErrInfo);//融资融券账户直接还款


//普通批量版新增的5个函数：(有些券商对批量操作进行了限速，最大批量操作数目请咨询券商)
// void  QueryHistoryData(int ClientID, int Category, char* StartDate, char* EndDate, char* Result, char* ErrInfo);//查询各类历史数据
// void  QueryDatas(int ClientID, int Category[], int Count, char* Result[], char* ErrInfo[]);//单账户批量查询各类交易数据
// void  SendOrders(int ClientID, int Category[] , int PriceType[], char* Gddm[],  char* Zqdm[] , float Price[], int Quantity[],  int Count, char* Result[], char* ErrInfo[]);//单账户批量下单
// void  CancelOrders(int ClientID, char* ExchangeID[], char* hth[], int Count, char* Result[], char* ErrInfo[]);//单账户批量撤单
// void  GetQuotes(int ClientID, char* Zqdm[], int Count, char* Result[], char* ErrInfo[]);//单账户批量获取五档报价


//高级批量版新增的4个函数：
// void  QueryMultiAccountsDatas(int ClientID[], int Category[], int Count, char* Result[], char* ErrInfo[]);//批量向不同账户查询各类交易数据
// void  SendMultiAccountsOrders(int ClientID[], int Category[] , int PriceType[], char* Gddm[],  char* Zqdm[] , float Price[], int Quantity[],  int Count, char* Result[], char* ErrInfo[]);//批量向不同账户下单
// void  CancelMultiAccountsOrders(int ClientID[], char* ExchangeID[], char* hth[], int Count, char* Result[], char* ErrInfo[]);//批量向不同账户撤单
// void  GetMultiAccountsQuotes(int ClientID[], char* Zqdm[], int Count, char* Result[], char* ErrInfo[]);//批量向不同账户获取五档报价


///交易接口执行后，如果失败，则字符串ErrInfo保存了出错信息中文说明；
///如果成功，则字符串Result保存了结果数据,形式为表格数据，行数据之间通过\n字符分割，列数据之间通过\t分隔。
///Result是\n，\t分隔的中文字符串，比如查询股东代码时返回的结果字符串就是 

///"股东代码\t股东名称\t帐号类别\t保留信息\n
///0000064567\t\t0\t\nA000064567\t\t1\t\n
///2000064567\t\t2\t\nB000064567\t\t3\t"

///查得此数据之后，通过分割字符串， 可以恢复为几行几列的表格形式的数据



//2.API使用流程为: 应用程序先调用OpenTdx打开通达信实例，一个实例下可以同时登录多个交易账户，每个交易账户称之为ClientID.
//通过调用Logon获得ClientID，然后可以调用其他API函数向各个ClientID进行查询或下单; 应用程序退出时应调用Logoff注销ClientID, 最后调用CloseTdx关闭通达信实例. 
//OpenTdx和CloseTdx在整个应用程序中只能被调用一次.API带有断线自动重连功能，应用程序只需根据API函数返回的出错信息进行适当错误处理即可。



    public static class TdxWrapper
    {
        //基本版

        /// <summary>
        /// 打开通达信实例
        /// </summary>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void OpenTdx();

        /// <summary>
        /// 关闭通达信实例
        /// </summary>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void CloseTdx();

        /// <summary>
        /// 交易账户登录
        /// </summary>
        /// <param name="IP">券商交易服务器IP</param>
        /// <param name="Port">券商交易服务器端口</param>
        /// <param name="Version">设置通达信客户端的版本号</param>
        /// <param name="YybID">营业部代码，请到网址 http://www.chaoguwaigua.com/downloads/qszl.htm 查询</param>
        /// <param name="AccountNo">完整的登录账号，券商一般使用资金帐户或客户号</param>
        /// <param name="TradeAccount">交易账号，一般与登录帐号相同. 请登录券商通达信软件，查询股东列表，股东列表内的资金帐号就是交易帐号, 具体查询方法请见网站“热点问答”栏目</param>
        /// <param name="JyPassword">交易密码</param>
        /// <param name="TxPassword">通讯密码</param>
        /// <param name="ErrInfo">此API执行返回后，如果出错，保存了错误信息说明。一般要分配256字节的空间。没出错时为空字符串。</param>
        /// <returns>客户端ID，失败时返回-1</returns>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Logon(string IP, short Port, string Version, short YybID, string AccountNo, string TradeAccount, string JyPassword, string TxPassword, StringBuilder ErrInfo);
        
        /// <summary>
        /// 交易账户注销
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Logoff(int ClientID);
        
        /// <summary>
        /// 查询各种交易数据
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        /// <param name="Category">表示查询信息的种类，0资金  1股份   2当日委托  3当日成交     4可撤单   5股东代码  6融资余额   7融券余额  8可融证券</param>
        /// <param name="Result">此API执行返回后，Result内保存了返回的查询数据, 形式为表格数据，行数据之间通过\n字符分割，列数据之间通过\t分隔。一般要分配1024*1024字节的空间。出错时为空字符串。</param>
        /// <param name="ErrInfo">同Logon函数的ErrInfo说明</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void QueryData(int ClientID, int Category, StringBuilder Result, StringBuilder ErrInfo);
        
        /// <summary>
        /// 下委托交易证券
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        /// <param name="Category">表示委托的种类，0买入 1卖出  2融资买入  3融券卖出   4买券还券   5卖券还款  6现券还券</param>
        /// <param name="PriceType">表示报价方式 0上海限价委托 深圳限价委托 1(市价委托)深圳对方最优价格  2(市价委托)深圳本方最优价格  3(市价委托)深圳即时成交剩余撤销  4(市价委托)上海五档即成剩撤 深圳五档即成剩撤 5(市价委托)深圳全额成交或撤销 6(市价委托)上海五档即成转限价
        /// <param name="Gddm">股东代码, 交易上海股票填上海的股东代码；交易深圳的股票填入深圳的股东代码</param>
        /// <param name="Zqdm">证券代码</param>
        /// <param name="Price">委托价格</param>
        /// <param name="Quantity">委托数量</param>
        /// <param name="Result">同上,其中含有委托编号数据</param>
        /// <param name="ErrInfo">同上</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SendOrder(int ClientID, int Category, int PriceType, string Gddm, string Zqdm, float Price, int Quantity, StringBuilder Result, StringBuilder ErrInfo);
        
        /// <summary>
        /// 撤委托
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        /// <param name="ExchangeID">交易所ID， 上海1，深圳0(招商证券普通账户深圳是2)</param>
        /// <param name="hth">表示要撤的目标委托的编号</param>
        /// <param name="Result">同上</param>
        /// <param name="ErrInfo">同上</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void CancelOrder(int ClientID, string ExchangeID, string hth, StringBuilder Result, StringBuilder ErrInfo);
        
        /// <summary>
        /// 获取证券的实时五档行情
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        /// <param name="Zqdm">证券代码</param>
        /// <param name="Result">同上</param>
        /// <param name="ErrInfo">同上</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void GetQuote(int ClientID, string Zqdm, StringBuilder Result, StringBuilder ErrInfo);
        
        /// <summary>
        /// 融资融券直接还款
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        /// <param name="Amount">还款金额</param>
        /// <param name="Result">同上</param>
        /// <param name="ErrInfo">同上</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Repay(int ClientID, string Amount, StringBuilder Result, StringBuilder ErrInfo);


        ///普通批量版

        /// <summary>
        /// 属于普通批量版功能,查询各种历史数据
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        /// <param name="Category">表示查询信息的种类，0历史委托  1历史成交   2交割单</param>
        /// <param name="StartDate">表示开始日期，格式为yyyyMMdd,比如2014年3月1日为  20140301
        /// <param name="EndDate">表示结束日期，格式为yyyyMMdd,比如2014年3月1日为  20140301
        /// <param name="Result">同上</param>
        /// <param name="ErrInfo">同上</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void QueryHistoryData(int ClientID, int Category, string StartDate, string EndDate, StringBuilder Result, StringBuilder ErrInfo);
        
        /// <summary>
        /// 属于普通批量版功能,批量查询各种交易数据,用数组传入每个委托的参数，数组第i个元素表示第i个查询的相应参数
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        /// <param name="Category">信息的种类的数组, 第i个元素表示第i个查询的信息种类，0资金  1股份   2当日委托  3当日成交     4可撤单   5股东代码  6融资余额   7融券余额  8可融证券</param>
        /// <param name="Count">查询的个数，即数组的长度</param>
        /// <param name="Result">返回数据的数组, 第i个元素表示第i个委托的返回信息. 此API执行返回后，Result[i]含义同上。</param>
        /// <param name="ErrInfo">错误信息的数组，第i个元素表示第i个委托的错误信息. 此API执行返回后，ErrInfo[i]含义同上。</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void QueryDatas(int ClientID,
            int[] Category, 
            int Count,
            IntPtr[] Result,
            IntPtr[] ErrInfo);
        
        /// <summary>
        /// 属于普通批量版功能,批量下委托交易证券，用数组传入每个委托的参数，数组第i个元素表示第i个委托的相应参数
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        /// <param name="Category">委托种类的数组，第i个元素表示第i个委托的种类，0买入 1卖出  2融资买入  3融券卖出   4买券还券   5卖券还款  6现券还券</param>
        /// <param name="PriceType">表示报价方式的数组,  第i个元素表示第i个委托的报价方式, 0上海限价委托 深圳限价委托 1(市价委托)深圳对方最优价格  2(市价委托)深圳本方最优价格  3(市价委托)深圳即时成交剩余撤销  4(市价委托)上海五档即成剩撤 深圳五档即成剩撤 5(市价委托)深圳全额成交或撤销 6(市价委托)上海五档即成转限价
        /// <param name="Gddm">股东代码数组，第i个元素表示第i个委托的股东代码，交易上海股票填上海的股东代码；交易深圳的股票填入深圳的股东代码</param>
        /// <param name="Zqdm">证券代码数组，第i个元素表示第i个委托的证券代码</param>
        /// <param name="Price">委托价格数组，第i个元素表示第i个委托的委托价格</param>
        /// <param name="Quantity">委托数量数组，第i个元素表示第i个委托的委托数量</param>
        /// <param name="Count">委托的个数，即数组的长度</param>
        /// <param name="Result">同上</param>
        /// <param name="ErrInfo">同上</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SendOrders(int ClientID, int[] Category, int[] PriceType, string[] Gddm, string[] Zqdm, float[] Price, int[] Quantity, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        
        /// <summary>
        /// 属于普通批量版功能,批量撤委托, 用数组传入每个委托的参数，数组第i个元素表示第i个撤委托的相应参数
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        /// <param name="ExchangeID">交易所ID， 上海1，深圳0(招商证券普通账户深圳是2)</param>
        /// <param name="hth">表示要撤的目标委托的编号</param>
        /// <param name="Count">撤委托的个数，即数组的长度</param>
        /// <param name="Result">同上</param>
        /// <param name="ErrInfo">同上</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void CancelOrders(int ClientID, string[] ExchangeID, string[] hth, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        
        /// <summary>
        /// 属于普通批量版功能,批量获取证券的实时五档行情
        /// </summary>
        /// <param name="ClientID">客户端ID</param>
        /// <param name="Zqdm">证券代码</param>
        /// <param name="Result">同上</param>
        /// <param name="ErrInfo">同上</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void GetQuotes(int ClientID, string[] Zqdm, int Count, IntPtr[] Result, IntPtr[] ErrInfo);


        ///高级批量版
       
        /// <summary>
        /// 属于高级批量版功能,批量查询各种交易数据,用数组传入每个委托的参数，数组第i个元素表示第i个查询的相应参数
        /// </summary>
        /// <param name="ClientID">客户端ID数组,第i个元素表示第i个查询的客户端ID</param>
        /// <param name="Category">信息的种类的数组, 第i个元素表示第i个查询的信息种类，0资金  1股份   2当日委托  3当日成交     4可撤单   5股东代码  6融资余额   7融券余额  8可融证券</param>
        /// <param name="Count">查询的个数，即数组的长度</param>
        /// <param name="Result">返回数据的数组, 第i个元素表示第i个委托的返回信息. 此API执行返回后，Result[i]含义同上。</param>
        /// <param name="ErrInfo">错误信息的数组，第i个元素表示第i个委托的错误信息. 此API执行返回后，ErrInfo[i]含义同上。</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void QueryMultiAccountsDatas(int[] ClientID, int[] Category, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        
        /// <summary>
        /// 属于高级批量版功能,批量下委托交易证券，用数组传入每个委托的参数，数组第i个元素表示第i个委托的相应参数
        /// </summary>
        /// <param name="ClientID">客户端ID数组,第i个元素表示第i个委托的客户端ID</param>
        /// <param name="Category">委托种类的数组，第i个元素表示第i个委托的种类，0买入 1卖出  2融资买入  3融券卖出   4买券还券   5卖券还款  6现券还券</param>
        /// <param name="PriceType">表示报价方式的数组,  第i个元素表示第i个委托的报价方式, 0上海限价委托 深圳限价委托 1(市价委托)深圳对方最优价格  2(市价委托)深圳本方最优价格  3(市价委托)深圳即时成交剩余撤销  4(市价委托)上海五档即成剩撤 深圳五档即成剩撤 5(市价委托)深圳全额成交或撤销 6(市价委托)上海五档即成转限价
        /// <param name="Gddm">股东代码数组，第i个元素表示第i个委托的股东代码，交易上海股票填上海的股东代码；交易深圳的股票填入深圳的股东代码</param>
        /// <param name="Zqdm">证券代码数组，第i个元素表示第i个委托的证券代码</param>
        /// <param name="Price">委托价格数组，第i个元素表示第i个委托的委托价格</param>
        /// <param name="Quantity">委托数量数组，第i个元素表示第i个委托的委托数量</param>
        /// <param name="Count">委托的个数，即数组的长度</param>
        /// <param name="Result">同上</param>
        /// <param name="ErrInfo">同上</param>
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SendMultiAccountsOrders(int[] ClientID, int[] Category, int[] PriceType, string[] Gddm, string[] Zqdm, float[] Price, int[] Quantity, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        
        /// <summary>
        /// 属于高级批量版功能,批量撤委托, 用数组传入每个委托的参数，数组第i个元素表示第i个撤委托的相应参数
        /// </summary>
        /// <param name="ClientID">客户端ID数组,第i个元素表示第i个撤单的客户端ID</param>
        /// <param name="ExchangeID">交易所ID， 上海1，深圳0(招商证券普通账户深圳是2)</param>
        /// <param name="hth">表示要撤的目标委托的编号</param>
        /// <param name="Count">撤委托的个数，即数组的长度</param>
        /// <param name="Result">同上</param>
        /// <param name="ErrInfo">同上</param>        
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void CancelMultiAccountsOrders(int[] ClientID, string[] ExchangeID, string[] hth, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        
        /// 属于高级批量版功能,批量获取证券的实时五档行情
        /// </summary>
        /// <param name="ClientID">客户端ID数组,第i个元素表示第i个查询的客户端ID</param>
        /// <param name="Zqdm">证券代码</param>
        /// <param name="Result">同上</param>
        /// <param name="ErrInfo">同上</param>        
        [DllImport("trade.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void GetMultiAccountsQuotes(int[] ClientID, string[] Zqdm, int Count, IntPtr[] Result, IntPtr[] ErrInfo);


        public static string DecryptAccount(string encryptedAccount)
        {
            int len = encryptedAccount.Length;

            if (len % 2 != 0)
            {
                throw new ArgumentException("encryptedAccount");
            }

            len /= 2;

            ushort[] temp = new ushort[len];

            for (int i = 0; i < len; ++i)
            {
                temp[i] = (ushort)((encryptedAccount[2 * i] - 'A') * 26 + (encryptedAccount[2*i + 1] - 'A'));
            }

            ushort key = 0x55E;
            for (int i = 0; i < len; ++i)
            {
                ushort v = temp[i];
    
                temp[i] = (ushort)((key >> 8) ^ v);
                key = (ushort)((((uint)v + (uint)key) * 0x5207F + 0x6adc3) & 0xFFFF);
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < temp.Length; ++i)
            {
                builder.Append((char)temp[i]);
            }

            return builder.ToString();
        }

        public static string EncryptAccount(string account)
        {
            if (string.IsNullOrEmpty(account))
            {
                throw new ArgumentNullException("account");
            }

            int len = account.Length;

            // get the characters in odd pos
            len /= 2;

            string oddPosAccount = string.Empty;

            for (int i = 0; i < len; ++i)
            {
                oddPosAccount += account[2 * i];
            }
            
            if (account.Length % 2 != 0)
            {
                ++len;
                oddPosAccount += account[2 * len - 2];
            }

            ushort[] temp = new ushort[len];

            ushort key = 0x55E;
            for (int i = 0; i < len; ++i)
            {
                ushort v = (ushort)oddPosAccount[i];

                temp[i] = (ushort)((key >> 8) ^ v);
                key = (ushort)((((uint)temp[i] + (uint)key) * 0x5207F + 0x6adc3) & 0xFFFF);
            }

            byte[] encryptedAccount = new byte[len * 2];

            for (int i = 0; i < len; ++i)
            {
                byte a = (byte)(temp[i] / 26);
                byte b = (byte)(temp[i] % 26);

                encryptedAccount[2 * i] = (byte)(a + 'A');
                encryptedAccount[2 * i + 1] = (byte)(b + 'A');
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < encryptedAccount.Length; ++i)
            {
                builder.Append((char)encryptedAccount[i]);
            }

            return builder.ToString();
        }
    }
}
