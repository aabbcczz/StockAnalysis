namespace AutoExportStockData
{
    using System;
    using System.IO;
    using System.Configuration;

    using AutoIt;
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No program is specified");
                Environment.Exit(1);
            }

            try
            {
                string account = ConfigurationManager.AppSettings["Account"];
                string password = ConfigurationManager.AppSettings["Password"];

                Run(args, account, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
                Environment.Exit(1);
            }

#if DEBUG
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
#endif
        }

        static void Run(string[] args, string account, string password)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("args");
            }

            bool existsMainWindow = ExistsMainWindow();

            int processId = 0;

            string exePath = args[0];

            if (!existsMainWindow)
            {
                processId = AutoItX.Run(exePath, Path.GetDirectoryName(exePath));

                if (processId == 0)
                {
                    throw new InvalidOperationException(
                        string.Format("Failed to run {0}", exePath));
                }
            }
            else
            {
                Console.WriteLine("Main windows had been opened");
            }

            try
            {
                if (!existsMainWindow)
                {
                    Login(account, password);
                }

                bool isFutureData = args.Length > 1 && args[1] == "future";

                IDataExporter exporter = null;

                if (isFutureData)
                {
                    exporter = new FutureDataExporter();
                }
                else
                {
                    exporter = new StockDataExporter();
                }

                exporter.Export();
            }
            finally
            {
                if (processId != 0)
                {
                    AutoItX.ProcessClose(processId.ToString());
                }
            }


        }

        static void Login(string account, string password)
        {
            string title = ConfigurationManager.AppSettings["MainWindowTitle"];

            int handle = AutoItX.WinWait(title, "", 10);

            if (handle == 0)
            {
                throw new InvalidOperationException("failed to find login window");
            }

            IntPtr hwnd = AutoItX.WinGetHandle(title, "");

            AutoItX.WinActivate(hwnd);

            IntPtr hwndAccount = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:SafeEdit1]");
            IntPtr hwndPassword = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:SafeEdit2]");

            AutoItX.ControlSetText(hwnd, hwndAccount, account);
            AutoItX.ControlSetText(hwnd, hwndPassword, password);

            AutoItX.Sleep(2000); // wait 2s

            AutoItX.Send("{ENTER}");
        }

        static bool ExistsMainWindow()
        {
            string title = string.Format("[TITLE:{0}; CLASS:{1}]", ConfigurationManager.AppSettings["MainWindowTitle"], ConfigurationManager.AppSettings["MainWindowClass"]);

            int handle = AutoItX.WinWait(title, "", 2);

            return handle != 0;
        }
    }
}
