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

        //static void TestAccountEncryptionDecryption()
        //{
        //    string decryptedAccount = TdxWrapper.DecryptAccount("CAEZBMBJ");

        //    Console.WriteLine(decryptedAccount);

        //    string encryptedAccount = TdxWrapper.EncryptAccount("13003470");
        //    Console.WriteLine(encryptedAccount);

        //    string encryptedAccount1 = TdxWrapper.EncryptAccount("42000042387");
        //    Console.WriteLine(encryptedAccount1);

        //    string decryptedAccount1 = TdxWrapper.DecryptAccount(encryptedAccount1);
        //    Console.WriteLine(decryptedAccount1);
        //}

        //static void TestCalcLimit()
        //{
        //    float[] prices = new float[] { 21.93F, 77.0F, 13.07F, 7.49F, 101.21F  };

        //    foreach (var price in prices)
        //    {
        //        float upLimit = TradingHelper.CalcUpLimit(price);
        //        float downLimit = TradingHelper.CalcDownLimit(price);

        //        Console.WriteLine("Price: {0:0.000}, Up limit: {1:0.000}, Down limit: {2:0.000}", price, upLimit, downLimit);
        //    }
        //}

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
            // TestCalcLimit();

            TradingEnvironment.Initialize();

            try
            {
                using (var client = new TradingClient())
                {
                    string error;

                    while (true)
                    {
                        if (!client.LogOn("wt5.foundersc.com", 7708, "6.19", 1, "13003470", 9, "13003470", "789012", string.Empty, out error))
                        {
                            Console.WriteLine("Log on failed: {0}", error);
                        }
                        else
                        {
                            break;
                        }
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
        }
    }
}
