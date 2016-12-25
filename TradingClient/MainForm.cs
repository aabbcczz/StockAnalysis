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
using log4net;
using log4net.Repository.Hierarchy;

namespace TradingClient
{
    public partial class MainForm : Form
    {
        private bool _initialized = false;

        public MainForm()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string error;
            var result = CtpSimulator.GetInstance().QueryCapital(out error);

            if (result == null)
            {
                AppLogger.Default.FatalFormat("Failed to query capital", error);
            }
            else
            {
                AppLogger.Default.InfoFormat("total: {0}, usable: {1}, cashable: {2}, frozen: {3}", 
                    result.TotalEquity, result.UsableCapital, result.CashableCapital, result.FrozenCapital);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeLogger();

            if (!Initialize())
            {
                Close();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnInitialize();
        }

        private void InitializeLogger()
        {
            // initialize logger
            log4net.Config.XmlConfigurator.Configure();

            var appenders = AppLogger.Default.Logger.Repository.GetAppenders();
            foreach (var appender in appenders)
            {
                TextBoxAppender textBoxAppender = appender as TextBoxAppender;
                if (textBoxAppender != null)
                {
                    textBoxAppender.SetControl(textBoxLog);
                }
            }
        }

        private bool Initialize()
        {
            // initialize CTP simulator
            bool enableSinaQuote = false;
            string error;
            int retryCount = 3;
            bool initializeSucceeded = false;

            while (retryCount > 0)
            {
                if (!CtpSimulator.GetInstance().Initialize(enableSinaQuote, "202.108.253.186", 7708, "8.19", 1, "42000042387", 8, "42000042387", "789012", string.Empty, out error))
//                if (!CtpSimulator.GetInstance().Initialize(enableSinaQuote, "wt5.foundersc.com", 7708, "6.19", 1, "13003470", 9, "13003470", "789012", string.Empty, out error))
                {
                    string errorLog = string.Format("Initialize CtpSimulator failed, error: {0}", error);

                    AppLogger.Default.FatalFormat(errorLog);

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

            _initialized = initializeSucceeded;

            return initializeSucceeded;
        }

        private void UnInitialize()
        {
            if (_initialized)
            {
                CtpSimulator.GetInstance().UnInitialize();
            }
        }
    }
}
