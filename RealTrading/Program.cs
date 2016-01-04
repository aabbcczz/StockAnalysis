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
        static void WriteOutput(string result, string error)
        {
            Console.WriteLine("Result: {0}", result);
            Console.WriteLine("Error: {0}", error);
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

        static void Main(string[] args)
        {
            // TestAccountEncryptionDecryption();

            TradingEnvironment.Initialize();

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

                    string result;

                    client.QueryData(DataCategory.Capital, out result, out error);
                    WriteOutput(result, error);

                    client.QueryData(DataCategory.OrderSentToday, out result, out error);
                    WriteOutput(result, error);

                    client.QueryData(DataCategory.SucceededOrderTody, out result, out error);
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

                    client.QueryData(DataCategory.MarginableSecuirty, out result, out error);
                    WriteOutput(result, error);


                    for (;;)
                    {
                        client.GetQuote("000001", out result, out error);

                        WriteOutput(result, error);

                        Thread.Sleep(2000);
                    }
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
