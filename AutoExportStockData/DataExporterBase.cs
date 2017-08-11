using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoIt;

namespace AutoExportStockData
{
    abstract class DataExporterBase : IDataExporter
    {
        private IntPtr GetQuoteButtonHandle(IntPtr hwnd)
        {
            string IdString = string.Format("[ID:{0}]", ConfigurationManager.AppSettings["QuoteButtonControlId"]);
            IntPtr buttonQuoteHandle = AutoItX.ControlGetHandle(hwnd, IdString);
            if (buttonQuoteHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("failed to find 行情 button");
            }

            return buttonQuoteHandle;
        }

        private IntPtr GetMenuButtonHandle(IntPtr hwnd)
        {
            // find out main menu entry
            string IdString = string.Format("[ID:{0}]", ConfigurationManager.AppSettings["MenuButtonControlId"]);
            IntPtr hwndMenu = AutoItX.ControlGetHandle(hwnd, IdString);

            if (hwndMenu == IntPtr.Zero)
            {
                throw new InvalidOperationException("Can't find main menu button");
            }

            return hwndMenu;
        }

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

            IntPtr buttonQuoteHandle = GetQuoteButtonHandle(hwnd);

            // click 行情 twice to ensure menu item "export data" is activated
            AutoItX.ControlClick(hwnd, buttonQuoteHandle);
            AutoItX.Sleep(3000);
            AutoItX.ControlClick(hwnd, buttonQuoteHandle);
            AutoItX.Sleep(3000);

            ExportData(hwnd);
        }

        void ExportData(IntPtr hwndMain)
        {
            IntPtr hwndMenu = GetMenuButtonHandle(hwndMain);

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

            int step = 1;
            do
            {
                AutoItX.WinActivate(hwnd);
                AutoItX.ControlClick(hwnd, buttonAddObject);

                if (SelectDataToExport(step))
                {
                    ++step;
                }
                else
                {
                    break;
                }
            } while (true);

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

        abstract protected bool SelectDataToExport(int step);

        abstract protected void SelectDataToDownload(IntPtr hwnd);

        void DownloadQuoteAction()
        {
            string title = "盘后数据下载";

            int handle = AutoItX.WinWait(title, "", 10);

            if (handle == 0)
            {
                throw new InvalidOperationException("failed to find dialog for downloading quote");
            }

            IntPtr hwnd = AutoItX.WinGetHandle(title, "");

            AutoItX.WinActivate(hwnd);

            SelectDataToDownload(hwnd);

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
            IntPtr hwndMenu = GetMenuButtonHandle(hwndMain);

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

            DownloadQuoteAction();
        }
    }
}
