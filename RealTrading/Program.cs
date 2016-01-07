using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;

namespace RealTrading
{
    class Program
    {
        static void WriteOutput(TabulateData result, string error)
        {
            Console.WriteLine("Error: {0}", error);
            Console.WriteLine("Result:");
            
            if (result == null)
            {
                return;
            }

            var columns = result.Columns;

            foreach (var column in columns)
            {
                Console.Write("{0}\t", column);
            }

            Console.WriteLine();

            foreach (var row in result.Rows)
            {
                foreach (var field in row)
                {
                    Console.Write("{0}\t", field);
                }
            }

            Console.WriteLine();
        }

        static void TestAccountEncryptionDecryption()
        {
            string decryptedAccount = TdxWrapper.DecryptAccount("CAEZBMBJ");

            Console.WriteLine(decryptedAccount);

            string encryptedAccount = TdxWrapper.EncryptAccount("13003470");
            Console.WriteLine(encryptedAccount);

            string encryptedAccount1 = TdxWrapper.EncryptAccount("42000042387");
            Console.WriteLine(encryptedAccount1);

            string decryptedAccount1 = TdxWrapper.DecryptAccount(encryptedAccount1);
            Console.WriteLine(decryptedAccount1);
        }

        static void TestCalcLimit()
        {
            float[] prices = new float[] { 21.93F, 77.0F, 13.07F, 7.49F, 101.21F  };

            foreach (var price in prices)
            {
                float upLimit = TradingHelper.CalcUpLimit(price);
                float downLimit = TradingHelper.CalcDownLimit(price);

                Console.WriteLine("Price: {0:0.000}, Up limit: {1:0.000}, Down limit: {2:0.000}", price, upLimit, downLimit);
            }
        }

        static void ShowQuote(TabulateData result, DateTime time)
        {
            foreach (var quote in FiveLevelQuote.ExtractFrom(result, time))
            {
                Console.WriteLine("{0} {1} {2} {3:0.000} {4:0.000} {5:0.000}", quote.Timestamp, quote.SecurityCode, quote.SecurityName, quote.YesterdayClosePrice, quote.TodayOpenPrice, quote.CurrentPrice);
                for (int j = quote.AskPrices.Length - 1; j >= 0; --j)
                {
                    Console.WriteLine("{0:0.000} {1}", quote.AskPrices[j], quote.AskVolumes[j]);
                }

                Console.WriteLine("-----------------");

                for (int j = 0; j < quote.BidPrices.Length; ++j)
                {
                    Console.WriteLine("{0:0.000} {1}", quote.BidPrices[j], quote.BidVolumes[j]);
                }

                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            // TestAccountEncryptionDecryption();

            TradingEnvironment.Initialize();

            TestCalcLimit();

            try
            {
                using (var client = new TradingClient())
                {
                    string error;
                    if (!client.LogOn("wt5.foundersc.com", 7708, "6.19", 1, "13003470", 9, "13003470", "789012", string.Empty, out error))
                    {
                        throw new InvalidOperationException(string.Format("Log on failed: {0}", error));
                    }

                    Console.WriteLine("Logged on, client id = {0}", client.ClientId);

                    TabulateData result;

                    client.QueryData(DataCategory.Capital, out result, out error);
                    WriteOutput(result, error);

                    client.QueryData(DataCategory.OrderSubmittedToday, out result, out error);
                    WriteOutput(result, error);

                    client.QueryData(DataCategory.OrderSucceededToday, out result, out error);
                    WriteOutput(result, error);

                    client.QueryData(DataCategory.Stock, out result, out error);
                    WriteOutput(result, error);

                    client.QueryData(DataCategory.ShareholderRegistryCode, out result, out error);
                    WriteOutput(result, error);

                    client.QueryData(DataCategory.CancellableOrder, out result, out error);
                    WriteOutput(result, error);

                    client.QueryData(DataCategory.FinancingBalance, out result, out error);
                    WriteOutput(result, error);

                    client.QueryData(DataCategory.MarginBalance, out result, out error);
                    WriteOutput(result, error);

                    client.QueryData(DataCategory.MarginableSecurity, out result, out error);
                    WriteOutput(result, error);


                    Console.WriteLine(">>>>>>> test array api");

                    TabulateData[] results;
                    string[] errors;
                    DataCategory[] categories = new DataCategory[]
                    {
                        DataCategory.Capital,
                        DataCategory.OrderSubmittedToday,
                        DataCategory.OrderSucceededToday,
                        DataCategory.Stock,
                        DataCategory.ShareholderRegistryCode,
                        DataCategory.CancellableOrder,
                    };


                    bool[] succeeds = client.QueryData(categories, out results, out errors);
                    for(int i = 0; i < categories.Length; ++i)
                    {
                        WriteOutput(results[i], errors[i]);
                    }
                    
                    
                    string[] codes = new string[]{ "000001", "000004", "601398" };

                    for (int k = 0; k < 20; k++)
                    {
                        succeeds = client.GetQuote(codes, out results, out errors);
                        for (int i = 0; i < codes.Length; ++i)
                        {
                            if (succeeds[i])
                            {
                                ShowQuote(results[i], DateTime.Now);
                            }
                            else
                            {
                                Console.WriteLine("error: {0}", errors[i]);
                            }
                        }

                        Thread.Sleep(1000);

                        //DateTime now = DateTime.Now;
                        //if (now.TimeOfDay > new TimeSpan(9, 25, 1))
                        //{
                        //    break;
                        //}
                    }
                    
                    bool succeeded;

                    //DateTime time;

                    //for (; ; )
                    //{
                    //    Console.WriteLine("Please input code.");
                    //    string line = Console.ReadLine();

                    //    time = DateTime.Now;
                    //    string code = line.Trim();

                    //    if (code == "exit")
                    //    {
                    //        break;
                    //    }

                    //    if (client.GetQuote(code, out result, out error))
                    //    {
                    //        ShowQuote(result, time);
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine("error: {0}", error);
                    //    }
                    //}

                    //string icbcCode = "601398";

                    //time = DateTime.Now;
                    //if (!client.GetQuote(icbcCode, out result, out error))
                    //{
                    //    Console.WriteLine("error: {0}", error);
                    //}

                    //ShowQuote(result, time);

                    //var quotes = FiveLevelQuote.ExtractFrom(result, time);
                    //if (quotes.Count() == 0)
                    //{
                    //    Console.WriteLine("unable to get quote for {0}", icbcCode);
                    //}
                    //else
                    //{
                    //    var quote = quotes.First();

                    //    float yesterdayClosePrice = quote.YesterdayClosePrice;
                    //    float downLimit = PriceHelper.CalcDownLimit(yesterdayClosePrice);

                    //    Console.WriteLine("try to send order to buy 601398 at price {0:0.000}", downLimit);

                    //    bool succeeded = client.SendOrder(OrderCategory.Buy, OrderPriceType.LimitPrice, icbcCode, downLimit, 100, out result, out error);
                    //    WriteOutput(result, error);

                    //    if (succeeded)
                    //    {
                    //        var sendOrderResults = SendOrderResult.ExtractFrom(result);
                    //        if (sendOrderResults.Count() == 0)
                    //        {
                    //            Console.WriteLine("Failed to get result for SendOrder()");
                    //        }
                    //        else
                    //        {
                    //            var sendOrderResult = sendOrderResults.First();

                    //            Thread.Sleep(5000);

                    //            Console.WriteLine("try to cancel order {0}", sendOrderResult.OrderNo);

                    //            succeeded = client.CancelOrder(Exchange.GetTradeableExchangeForSecurity(icbcCode), sendOrderResult.OrderNo, out result, out error);

                    //            WriteOutput(result, error);
                    //        }
                    //    }
                    //}

                    succeeded = client.QueryData(DataCategory.OrderSubmittedToday, out result, out error);
                    if (succeeded)
                    {
                        var orders = QueryGeneralOrderResult.ExtractFrom(result);

                        foreach (var order in orders)
                        {
                            Console.WriteLine("{0} {1} {2} {3} {4:0.000} {5} {6} {7}",
                                order.OrderNo,
                                order.SubmissionTime,
                                order.BuySellFlag,
                                order.Status,
                                order.SubmissionPrice,
                                order.SubmissionVolume,
                                order.SubmissionType,
                                order.PricingType);
                        }
                    }

                    WriteOutput(result, error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex);
            }
            finally
            {
                TradingEnvironment.UnInitialize();
            }

#if DEBUG
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
#endif

     	    //DLL是32位的,因此必须把C#工程生成的目标平台从Any CPU改为X86,才能调用DLL;
            //必须把Trade.dll等4个DLL复制到Debug和Release工程目录下;
            //无论用什么语言编程，都必须仔细阅读VC版内的关于DLL导出函数的功能和参数含义说明，不仔细阅读完就提出问题者因时间精力所限，恕不解答。

        //    StringBuilder ErrInfo=new StringBuilder(256);
        //    StringBuilder Result = new StringBuilder(1024*1024);


  


        //    TdxWrapper.OpenTdx();//打开通达信

        //    //登录
        //    int ClientID = TdxWrapper.Logon("119.145.12.70", 443, "2.20", 0, "1111", "1111", "1111", string.Empty, ErrInfo);

        //    //登录第二个帐号
        //    //int ClientID2 = Logon("111.111.111.111", 7708, "4.20", 0, "5555555555", "1111", "555", string.Empty, ErrInfo);



        //    if (ClientID==-1)
        //    {
        //        Console.WriteLine(ErrInfo);
        //        return;
        //    }

        //    TdxWrapper.SendOrder(ClientID, 0, 0, "A000001", "601988", 2.5f, 100, Result, ErrInfo);//下单
        ////SendOrder(ClientID2, 0, 0, "A000001", "601988", 2.5f, 100, Result, ErrInfo);//第二个帐号,下单
        //    Console.WriteLine("下单结果: {0}", Result);


        //    TdxWrapper.GetQuote(ClientID, "601988", Result, ErrInfo);//查询五档报价
        //    if (ErrInfo.ToString() != string.Empty)
        //    {
        //        Console.WriteLine(ErrInfo.ToString());
        //        return;
        //    }
        //    Console.WriteLine("行情结果: {0}", Result);





        //    TdxWrapper.QueryData(ClientID, 0, Result, ErrInfo);//查询资金
        //    if (ErrInfo.ToString()!=string.Empty)
        //    {
        //        Console.WriteLine(ErrInfo.ToString());
        //        return;
        //    }
        //    Console.WriteLine("查询结果: {0}", Result);


        ////批量查询多个证券的五档报价
        //string[] Zqdm=new string[]{"600030","600031"};
        //string[] Results = new string[Zqdm.Length];
        //    string[] ErrInfos = new string[Zqdm.Length];

        //    IntPtr[] ResultPtr = new IntPtr[Zqdm.Length];
        //    IntPtr[] ErrInfoPtr = new IntPtr[Zqdm.Length];

        //    for (int i = 0; i < Zqdm.Length; i++)
        //    {
        //        ResultPtr[i] = Marshal.AllocHGlobal(1024 * 1024);
        //        ErrInfoPtr[i] = Marshal.AllocHGlobal(256);
        //    }




        //    TdxWrapper.GetQuotes(ClientID, Zqdm, Zqdm.Length, ResultPtr, ErrInfoPtr);

        //    for (int i = 0; i < Zqdm.Length; i++)
        //    {
        //        Results[i] =Marshal.PtrToStringAnsi(ResultPtr[i]);
        //        ErrInfos[i] = Marshal.PtrToStringAnsi(ErrInfoPtr[i]);

        //        Marshal.FreeHGlobal(ResultPtr[i]);
        //        Marshal.FreeHGlobal(ErrInfoPtr[i]);
        //    }






        //    TdxWrapper.Logoff(ClientID);//注销
        //    TdxWrapper.CloseTdx();//关闭通达信

        //    Console.ReadLine();
        }
    }
}
