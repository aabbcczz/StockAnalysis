using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoIt;

namespace AutoExportStockData
{
    class FutureDataExporter : DataExporterBase
    {
        protected override bool SelectDataToExport(int step)
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

            switch (step)
            {
                case 1:
                    DataExporterHelper.SelectListViewItem(hwnd, listView, "中金所期货");
                    break;
                case 2:
                    DataExporterHelper.SelectListViewItem(hwnd, listView, "郑州商品");
                    break;
                case 3:
                    DataExporterHelper.SelectListViewItem(hwnd, listView, "上海商品");
                    break;
                case 4:
                    DataExporterHelper.SelectListViewItem(hwnd, listView, "大连商品");
                    break;
                default:
                    AutoItX.WinClose(hwnd);
                    return false;
            }

            AutoItX.ControlClick(hwnd, buttonSelectAll);
            AutoItX.Sleep(1000);
            AutoItX.ControlClick(hwnd, buttonOk);
            AutoItX.Sleep(1000);

            return true;
        }

        protected override void SelectDataToDownload(IntPtr hwnd)
        {
            IntPtr tabControl = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:SysTabControl321]");
            for (int i = 0; i < 3; ++i)
            {
                AutoItX.ControlCommand(hwnd, tabControl, "TabRight", "");
                AutoItX.Sleep(1000);
            }


            // wait for a dialog "连接扩展行情" to disappear
            string title = "连接扩展市场行情主站";
            int handle = AutoItX.WinWait(title, "", 5);
            if (handle != 0)
            {
                IntPtr hwnd1 = AutoItX.WinGetHandle(title, "");
                if (hwnd1 != IntPtr.Zero)
                {
                    AutoItX.WinWaitClose(hwnd1);
                }
            }

            var rectControlInScreen = DataExporterHelper.GetControlPosInScreen(hwnd, tabControl);

            // select the "日线数据" checkbox
            IntPtr checkboxDailyDataHandle = AutoItX.ControlGetHandle(hwnd, "[ID:1427]"); //0x593
            if (checkboxDailyDataHandle != IntPtr.Zero)
            {
                AutoItX.ControlCommand(hwnd, checkboxDailyDataHandle, "Check", "");
            }
            else
            {
                throw new InvalidOperationException("failed to find 日线数据 button");
            }

            AutoItX.Sleep(1000);

            // select the "下载所有扩展行情数据" checkbox
            IntPtr checkboxDownloadAllExtendHistoryHandle = AutoItX.ControlGetHandle(hwnd, "[ID:1420]"); //0x58c
            if (checkboxDownloadAllExtendHistoryHandle != IntPtr.Zero)
            {
                AutoItX.ControlCommand(hwnd, checkboxDownloadAllExtendHistoryHandle, "Check", "");
            }
            else
            {
                throw new InvalidOperationException("failed to find 下载所有扩展行情数据 button");
            }

            AutoItX.Sleep(1000);

            // select the dropbox of "国内期货"
            IntPtr comboDomesticFutureHandle = AutoItX.ControlGetHandle(hwnd, "[ID:2087]"); //0x827
            if (comboDomesticFutureHandle != IntPtr.Zero)
            {
                AutoItX.ControlCommand(hwnd, comboDomesticFutureHandle, "SelectString", "国内期货");
            }
            else
            {
                throw new InvalidOperationException("failed to find 国内期货 button");
            }

            AutoItX.Sleep(1000);
        }
    }
}
