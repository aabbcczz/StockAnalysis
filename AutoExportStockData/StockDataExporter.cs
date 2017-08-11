using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoIt;

namespace AutoExportStockData
{
    class StockDataExporter : IDataExporter
    {
        public void Export()
        {
            IntPtr hwnd = DataExporterHelper.CleanUpAndGetMainWindowHandle();

            DownloadQuote(hwnd);

            // export data.
            // show a stock to enable menu item.
            if (AutoItX.WinActivate(hwnd) == 0)
            {
                throw new InvalidOperationException("failed to activate main window");
            }

            IntPtr buttonQuoteHandle = AutoItX.ControlGetHandle(hwnd, "[ClassNN:AfxWnd428]");
            if (buttonQuoteHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("failed to find 行情 button");
            }

            // click 行情 twice to ensure menu item "export data" is activated
            AutoItX.ControlClick(hwnd, buttonQuoteHandle);
            AutoItX.Sleep(3000);
            AutoItX.ControlClick(hwnd, buttonQuoteHandle);
            AutoItX.Sleep(3000);

            ExportData(hwnd);
        }

        void ExportData(IntPtr hwndMain)
        {
            // find out main menu entry
            IntPtr hwndMenu = AutoItX.ControlGetHandle(hwndMain, "[CLASSNN:AfxWnd422]");

            if (hwndMenu == IntPtr.Zero)
            {
                throw new InvalidOperationException("Can't find main menu button");
            }

            // activate the window by click the menu and cancel it.
            AutoItX.ControlClick(hwndMain, hwndMenu);

            AutoItX.Sleep(1000); // wait menu to show.

            // goto the download quote menu item
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{RIGHT}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{DOWN}");

            AutoItX.Send("{ENTER}");

            ExportDataAction();
        }
        void ExportDataAction()
        {
            string title = "数据导出";

            int handle = AutoItX.WinWait(title, "", 10);

            if (handle == 0)
            {
                throw new InvalidOperationException("failed to find dialog for data exporting");
            }

            IntPtr hwnd = AutoItX.WinGetHandle(title, "");

            AutoItX.WinActivate(hwnd);

            IntPtr buttonAdvancedExport = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button6]");
            AutoItX.ControlClick(hwnd, buttonAdvancedExport);

            AdvancedExport();

        }

        void AdvancedExport()
        {
            string title = "高级导出";

            int handle = AutoItX.WinWait(title, "", 10);

            if (handle == 0)
            {
                throw new InvalidOperationException("failed to find dialog for advance data exporting");
            }

            IntPtr hwnd = AutoItX.WinGetHandle(title, "");

            AutoItX.WinActivate(hwnd);

            IntPtr buttonAddObject = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button5]");
            IntPtr buttonBeginExport = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button7]");
            IntPtr buttonClose = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button8]");
            IntPtr comboBoxRight = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:ComboBox3]");
            IntPtr buttonGenerateHeader = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button4]");

            AutoItX.ControlCommand(hwnd, buttonGenerateHeader, "Check", "");
            AutoItX.ControlCommand(hwnd, comboBoxRight, "SelectString", "前复权");

            AutoItX.Sleep(2000);

            AutoItX.WinActivate(hwnd);
            AutoItX.ControlClick(hwnd, buttonAddObject);

            SelectData(1);

            AutoItX.WinActivate(hwnd);
            AutoItX.ControlClick(hwnd, buttonAddObject);

            SelectData(2);

            AutoItX.WinActivate(hwnd);
            AutoItX.ControlClick(hwnd, buttonAddObject);

            SelectData(3);

            AutoItX.WinActivate(hwnd);
            AutoItX.ControlClick(hwnd, buttonAddObject);

            SelectData(4);

            // begin export
            AutoItX.WinActivate(hwnd);
            AutoItX.ControlClick(hwnd, buttonBeginExport);

            int confirmationDialogHandle = AutoItX.WinWait("TdxW");
            AutoItX.WinClose("TdxW");

            AutoItX.Sleep(1000);

            int finishDialogHandle = AutoItX.WinWait("TdxW");
            AutoItX.WinClose("TdxW");

            AutoItX.WinActivate(hwnd);
            AutoItX.ControlClick(hwnd, buttonClose);
        }

        void SelectData(int step)
        {
            string title = "选择品种";

            int handle = AutoItX.WinWait(title, "", 10);

            if (handle == 0)
            {
                throw new InvalidOperationException("failed to find dialog for selecting data");
            }

            IntPtr hwnd = AutoItX.WinGetHandle(title, "");

            AutoItX.WinActivate(hwnd);

            IntPtr buttonSelectAll = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button3]");
            IntPtr buttonOk = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button1]");
            IntPtr listView = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:SysListView321]");

            switch (step)
            {
                case 1:
                    AutoItX.ControlListView(hwnd, listView, "Select", "6", "");
                    AutoItX.ControlClick(hwnd, buttonSelectAll);
                    AutoItX.Sleep(1000);
                    AutoItX.ControlClick(hwnd, buttonOk);
                    break;
                case 2:
                    AutoItX.Send("399005");
                    AutoItX.Sleep(1000);
                    AutoItX.Send("{ENTER}");
                    AutoItX.Sleep(1000);
                    break;
                case 3:
                    AutoItX.Send("399006");
                    AutoItX.Sleep(1000);
                    AutoItX.Send("{ENTER}");
                    AutoItX.Sleep(1000);
                    break;
                case 4:
                    AutoItX.Send("399300");
                    AutoItX.Sleep(1000);
                    AutoItX.Send("{ENTER}");
                    AutoItX.Sleep(1000);
                    break;
                default:
                    AutoItX.WinClose(hwnd);
                    break;
            }

            AutoItX.Sleep(1000);
        }

        void DownloadQuote()
        {
            string title = "盘后数据下载";

            int handle = AutoItX.WinWait(title, "", 10);

            if (handle == 0)
            {
                throw new InvalidOperationException("failed to find dialog for downloading quote");
            }

            IntPtr hwnd = AutoItX.WinGetHandle(title, "");

            AutoItX.WinActivate(hwnd);

            // select the "日线和实时行情数据" checkbox
            AutoItX.Send("{SPACE}");

            // click the download button
            IntPtr buttonDownload = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button9]");
            AutoItX.ControlClick(hwnd, buttonDownload);

            AutoItX.Sleep(2000);

            // wait until the close button is enabled.
            IntPtr buttonClose = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button10]");
            while (AutoItX.ControlCommand(hwnd, buttonClose, "IsEnabled", "") == "0")
            {
                AutoItX.Sleep(1000);
            }

            AutoItX.ControlClick(hwnd, buttonClose);
        }

        void DownloadQuote(IntPtr hwndMain)
        {
            // find out main menu entry
            IntPtr hwndMenu = AutoItX.ControlGetHandle(hwndMain, "[CLASSNN:AfxWnd422]");

            if (hwndMenu == IntPtr.Zero)
            {
                throw new InvalidOperationException("Can't find main menu button");
            }

            // activate the window by click the menu and cancel it.
            AutoItX.ControlClick(hwndMain, hwndMenu);

            AutoItX.Sleep(1000); // wait menu to show.

            // goto the download quote menu item
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{RIGHT}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{DOWN}");

            AutoItX.Send("{ENTER}");

            DownloadQuote();
        }
    }
}
