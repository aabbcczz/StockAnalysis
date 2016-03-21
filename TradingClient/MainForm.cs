using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

using StockTrading.Utility;
using StockAnalysis.Share;

namespace TradingClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }


        private void WriteOutput(TabulateData result, string error)
        {
            textBox1.AppendText("\n");
            textBox1.AppendText(string.Format("Error: {0}", error));
            textBox1.AppendText("\n");
            textBox1.AppendText("Result:");
            
            if (result == null)
            {
                return;
            }

            var columns = result.Columns;

            textBox1.AppendText("\n");
            foreach (var column in columns)
            {
                textBox1.AppendText(string.Format("{0}\t", column));
            }

            textBox1.AppendText("\n");
            foreach (var row in result.Rows)
            {
                foreach (var field in row)
                {
                    textBox1.AppendText(string.Format("{0}\t", field));
                }
            }
        }

        //static void TestAccountEncryptionDecryption()
        //{
        //    string decryptedAccount = TdxWrapper.DecryptAccount("CAEZBMBJ");

        //    textBox1.AppendText(decryptedAccount);

        //    string encryptedAccount = TdxWrapper.EncryptAccount("13003470");
        //    textBox1.AppendText(encryptedAccount);

        //    string encryptedAccount1 = TdxWrapper.EncryptAccount("42000042387");
        //    textBox1.AppendText(encryptedAccount1);

        //    string decryptedAccount1 = TdxWrapper.DecryptAccount(encryptedAccount1);
        //    textBox1.AppendText(decryptedAccount1);
        //}

        //static void TestCalcLimit()
        //{
        //    float[] prices = new float[] { 21.93F, 77.0F, 13.07F, 7.49F, 101.21F  };

        //    foreach (var price in prices)
        //    {
        //        float upLimit = TradingHelper.CalcUpLimit(price);
        //        float downLimit = TradingHelper.CalcDownLimit(price);

        //        textBox1.AppendText("Price: {0:0.000}, Up limit: {1:0.000}, Down limit: {2:0.000}", price, upLimit, downLimit);
        //    }
        //}

        private void ShowQuote(TabulateData result, DateTime time)
        {
            foreach (var quote in FiveLevelQuote.ExtractFrom(result, time))
            {
                textBox1.AppendText("\n");

                textBox1.AppendText(string.Format("{0} {1} {2} {3:0.000} {4:0.000} {5:0.000}", quote.Timestamp, quote.SecurityCode, quote.SecurityName, quote.YesterdayClosePrice, quote.TodayOpenPrice, quote.CurrentPrice));
                for (int j = quote.SellPrices.Length - 1; j >= 0; --j)
                {
                    textBox1.AppendText("\n");
                    textBox1.AppendText(string.Format("{0:0.000} {1}", quote.SellPrices[j], quote.SellVolumesInHand[j]));
                }

                textBox1.AppendText("\n");
                textBox1.AppendText("-----------------");

                for (int j = 0; j < quote.BuyPrices.Length; ++j)
                {
                    textBox1.AppendText("\n");
                    textBox1.AppendText(string.Format("{0:0.000} {1}", quote.BuyPrices[j], quote.BuyVolumesInHand[j]));
                }
            }
        }

        private void ShowQuote(SinaStockQuote quote)
        {
            textBox1.AppendText("\n");

            if (quote == null)
            {
                textBox1.AppendText("null quote");

                return;
            }

            textBox1.AppendText(string.Format("{0} {1} {2} {3:0.000} {4:0.000} {5:0.000}", quote.QuoteTime, quote.SecurityCode, quote.SecurityName, quote.YesterdayClosePrice, quote.TodayOpenPrice, quote.CurrentPrice));
            for (int j = quote.SellPrices.Length - 1; j >= 0; --j)
            {
                textBox1.AppendText("\n");
                textBox1.AppendText(string.Format("{0:0.000} {1}", quote.SellPrices[j], quote.SellVolumesInHand[j]));
            }

            textBox1.AppendText("\n");
            textBox1.AppendText("-----------------");

            for (int j = 0; j < quote.BuyPrices.Length; ++j)
            {
                textBox1.AppendText("\n");
                textBox1.AppendText(string.Format("{0:0.000} {1}", quote.BuyPrices[j], quote.BuyVolumesInHand[j]));
            }
        }

        private void ShowQuote(IEnumerable<SinaStockQuote> quotes)
        {
            foreach (var quote in quotes)
            {
                ShowQuote(quote);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // TestAccountEncryptionDecryption();
            // TestCalcLimit();

            try
            {
                var sinaQuote = await SinaStockQuoteInterface.GetQuote("000001");
                ShowQuote(sinaQuote);

                string[] sinaCodes = new string[]
                {
                    "000001",
                    "000002",
                    "000003",
                    "000004"
                };

                var sinaQuotes = await SinaStockQuoteInterface.GetQuote(sinaCodes);
                ShowQuote(sinaQuotes);

                using (var client = new StockTrading.Utility.TradingClient())
                {
                    string error;

                    int logonFailureCount = 0;
                    while (logonFailureCount < 5)
                    {
                        if (!client.LogOn("wt5.foundersc.com", 7708, "6.19", 1, "13003470", 9, "13003470", "789012", string.Empty, out error))
                        {
                            textBox1.AppendText("\n");
                            textBox1.AppendText(string.Format("Log on failed: {0}", error));

                            logonFailureCount++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!client.IsLoggedOn())
                    {
                        return;
                    }

                    textBox1.AppendText("\n");
                    textBox1.AppendText(string.Format("Logged on, client id = {0}", client.ClientId));

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

                    textBox1.AppendText("\n");
                    textBox1.AppendText(">>>>>>> test array api");

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

                    for (int k = 0; k < 5; k++)
                    {
                        succeeds = client.GetQuote(codes, out results, out errors);
                        if (k == 0)
                        {
                            WriteOutput(results[0], errors[0]);
                        }

                        for (int i = 0; i < codes.Length; ++i)
                        {
                            if (succeeds[i])
                            {
                                ShowQuote(results[i], DateTime.Now);
                            }
                            else
                            {
                                textBox1.AppendText("\n");

                                textBox1.AppendText(string.Format("error: {0}", errors[i]));
                            }
                        }

                        Thread.Sleep(3000);

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
                    //    textBox1.AppendText("Please input code.");
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
                    //        textBox1.AppendText("error: {0}", error);
                    //    }
                    //}

                    //string icbcCode = "601398";

                    //time = DateTime.Now;
                    //if (!client.GetQuote(icbcCode, out result, out error))
                    //{
                    //    textBox1.AppendText("error: {0}", error);
                    //}

                    //ShowQuote(result, time);

                    //var quotes = FiveLevelQuote.ExtractFrom(result, time);
                    //if (quotes.Count() == 0)
                    //{
                    //    textBox1.AppendText("unable to get quote for {0}", icbcCode);
                    //}
                    //else
                    //{
                    //    var quote = quotes.First();

                    //    float yesterdayClosePrice = quote.YesterdayClosePrice;
                    //    float downLimit = PriceHelper.CalcDownLimit(yesterdayClosePrice);

                    //    textBox1.AppendText("try to send order to buy 601398 at price {0:0.000}", downLimit);

                    //    bool succeeded = client.SendOrder(OrderCategory.Buy, OrderPriceType.LimitPrice, icbcCode, downLimit, 100, out result, out error);
                    //    WriteOutput(result, error);

                    //    if (succeeded)
                    //    {
                    //        var sendOrderResults = SendOrderResult.ExtractFrom(result);
                    //        if (sendOrderResults.Count() == 0)
                    //        {
                    //            textBox1.AppendText("Failed to get result for SendOrder()");
                    //        }
                    //        else
                    //        {
                    //            var sendOrderResult = sendOrderResults.First();

                    //            Thread.Sleep(5000);

                    //            textBox1.AppendText("try to cancel order {0}", sendOrderResult.OrderNo);

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
                            textBox1.AppendText("\n");

                            textBox1.AppendText(
                                string.Format(
                                    "{0} {1} {2} {3} {4:0.000} {5} {6} {7}",
                                    order.OrderNo,
                                    order.SubmissionTime,
                                    order.BuySellFlag,
                                    order.StatusString,
                                    order.SubmissionPrice,
                                    order.SubmissionVolume,
                                    order.SubmissionType,
                                    order.PricingType));
                        }
                    }

                    WriteOutput(result, error);
                }
            }
            catch (Exception ex)
            {
                textBox1.AppendText("\n");

                textBox1.AppendText(string.Format("{0}", ex));
            }
            finally
            {
                TradingEnvironment.UnInitialize();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            bool enableSinaQuote = false;

            string error;

            TradingEnvironment.Initialize();

            int retryCount = 3;
            bool initializeSucceeded = false;

            while (retryCount > 0)
            {
                if (!CtpSimulator.GetInstance().Initialize(enableSinaQuote, "wt5.foundersc.com", 7708, "6.19", 1, "13003470", 9, "13003470", "789012", string.Empty, out error))
                {
                    string errorLog = string.Format("Initialize CtpSimulator failed, error: {0}", error);
                    Logger.FatalLogger.FatalFormat(errorLog);

                    MessageBox.Show(errorLog, "Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    --retryCount;

                    Thread.Sleep(1000);

                    continue;
                }
                else
                {
                    initializeSucceeded = true;
                    break;
                }
            }

            if (!initializeSucceeded)
            {
                Close();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CtpSimulator.GetInstance().UnInitialize();

            TradingEnvironment.UnInitialize();
        }
    }
}
