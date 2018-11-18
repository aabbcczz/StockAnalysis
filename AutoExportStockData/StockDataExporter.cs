namespace AutoExportStockData
{
    using System;
    using AutoIt;

    class StockDataExporter : DataExporterBase
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

            IntPtr buttonSelectAll = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button3]");
            IntPtr buttonOk = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:Button1]");
            IntPtr listView = AutoItX.ControlGetHandle(hwnd, "[CLASSNN:SysListView321]");

            switch (step)
            {
                case 1:
                    DataExporterHelper.SelectListViewItem(hwnd, listView, "沪深Ａ股");
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

            return (step > 0 && step < 5);
        }

        protected override void SelectDataToDownload(IntPtr hwnd)
        {
            // select the "日线和实时行情数据" checkbox  // 0x590 = 1424
            IntPtr checkboxDailyAndRealtimeDataHandle = AutoItX.ControlGetHandle(hwnd, "[ID:1424]"); //0x590
            if (checkboxDailyAndRealtimeDataHandle != IntPtr.Zero)
            {
                AutoItX.ControlCommand(hwnd, checkboxDailyAndRealtimeDataHandle, "Check", "");
            }
            else
            {
                throw new InvalidOperationException("failed to find 日线和实时行情数据 checkbox");
            }
        }
    }
}
