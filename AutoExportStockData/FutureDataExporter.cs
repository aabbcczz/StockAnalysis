using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoIt;

namespace AutoExportStockData
{
    class FutureDataExporter : IDataExporter
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

            IntPtr tabControl = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:SysTabControl321]");
            IntPtr buttonSelectAll = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button3]");
            IntPtr buttonOk = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button1]");
            IntPtr listView = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:SysListView321]");

            // select 扩展市场行情
            AutoItX.ControlClick(hwnd, tabControl, "left", 1, 534, 10);
            AutoItX.Sleep(1000);

            string itemIdStr = null;

            switch (step)
            {
                case 1:
                    itemIdStr = AutoItX.ControlListView(hwnd, listView, "FindItem", "中金所期货", ""); // 中金所期货
                    break;
                case 2:
                    itemIdStr = AutoItX.ControlListView(hwnd, listView, "FindItem", "郑州商品", ""); // 郑州商品
                    break;
                case 3:
                    itemIdStr = AutoItX.ControlListView(hwnd, listView, "FindItem", "上海商品", ""); // 上海商品
                    break;
                case 4:
                    itemIdStr = AutoItX.ControlListView(hwnd, listView, "FindItem", "大连商品", ""); // 大连商品
                    break;
                default:
                    AutoItX.WinClose(hwnd);
                    break;
            }

            bool succeeded = false;

            if (itemIdStr != null)
            {
                int itemId;
                if (int.TryParse(itemIdStr, out itemId))
                {
                    if (itemId >= 0)
                    {
                        AutoItX.ControlListView(hwnd, listView, "Select", itemId.ToString(), ""); 
                        AutoItX.Sleep(500);
                        AutoItX.ControlClick(hwnd, buttonSelectAll);
                        AutoItX.Sleep(1000);
                        AutoItX.ControlClick(hwnd, buttonOk);

                        succeeded = true;
                    }
                }
            }

            if (!succeeded)
            {
                throw new InvalidOperationException(
                    string.Format("failed to find item id for step {0}, result is {1}", step, itemIdStr));
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

            IntPtr tabControl = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:SysTabControl321]");
            AutoItX.ControlCommand(hwnd, tabControl, "TabRight", "");
            AutoItX.Sleep(1000);
            AutoItX.ControlCommand(hwnd, tabControl, "TabRight", "");
            AutoItX.Sleep(1000);
            AutoItX.ControlCommand(hwnd, tabControl, "TabRight", "");
            AutoItX.Sleep(1000);

            // wait for a dialog "连接扩展行情" to disappear
            string title1 = "连接扩展市场行情主站";
            int handle1 = AutoItX.WinWait(title1, "", 5);
            if (handle != 0)
            {
                IntPtr hwnd1 = AutoItX.WinGetHandle(title1, "");
                if (hwnd1 != IntPtr.Zero)
                {
                    AutoItX.WinWaitClose(hwnd1);
                }
            }

            var rectControlInScreen = DataExporterHelper.GetControlPosInScreen(hwnd, tabControl);

            // select the "日线数据" checkbox
            AutoItX.MouseClick("left", rectControlInScreen.X + 46, rectControlInScreen.Y + 66);
            AutoItX.Sleep(1000);

            // select the "下载所有扩展行情数据" checkbox
            AutoItX.MouseClick("left", rectControlInScreen.X + 46, rectControlInScreen.Y + 126);
            AutoItX.Sleep(1000);

            // select the dropbox of "国内期货"
            AutoItX.MouseClick("left", rectControlInScreen.X + 264, rectControlInScreen.Y + 126);
            AutoItX.Sleep(1000);

            // scroll up to the top and then down one
            AutoItX.Send("{UP}");
            AutoItX.Send("{UP}");
            AutoItX.Send("{UP}");
            AutoItX.Send("{UP}");
            AutoItX.Send("{UP}");
            AutoItX.Send("{UP}");
            AutoItX.Send("{DOWN}");
            AutoItX.Send("{RETURN}");

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
