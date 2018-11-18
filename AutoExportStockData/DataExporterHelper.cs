namespace AutoExportStockData
{
    using System;
    using System.Runtime.InteropServices;

    using AutoIt;
    using System.Configuration;
    static class DataExporterHelper
    {
        public static IntPtr CleanUpAndGetMainWindowHandle()
        {
            AutoItX.Sleep(3000);
            AutoItX.WinClose("消息标题");
            //            AutoItX.WinClose("消息标题:交易提示");

            string title = string.Format("[TITLE:{0}; CLASS:{1}]", ConfigurationManager.AppSettings["MainWindowTitle"], ConfigurationManager.AppSettings["MainWindowClass"]);

            int handle = AutoItX.WinWait(title, "", 300);

            if (handle == 0)
            {
                throw new InvalidOperationException("failed to find main window");
            }

            IntPtr hwnd = AutoItX.WinGetHandle(title, "");

            AutoItX.WinActivate(hwnd);

            // wait for the password/warning message box being closed automatically
            AutoItX.Sleep(15000);

            // close 中信证券消息中心 window
            AutoItX.WinClose("中信证券消息中心");

            return hwnd;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetClientRect(IntPtr hwnd, out Rect rect);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hwnd, ref System.Drawing.Point point);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        static System.Drawing.Rectangle GetClientRect(IntPtr handle)
        {
            Rect rect;
            GetClientRect(handle, out rect);
            System.Drawing.Point p = new System.Drawing.Point(0, 0);
            ClientToScreen(handle, ref p);
            rect.Left = (int)p.X;
            rect.Top = (int)p.Y;
            return new System.Drawing.Rectangle(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static System.Drawing.Rectangle GetControlPosInScreen(IntPtr hwnd, IntPtr hctrl)
        {
            var rectClient = GetClientRect(hwnd);
            var rectControl = AutoItX.ControlGetPos(hwnd, hctrl);

            return new System.Drawing.Rectangle(
                rectClient.X + rectControl.X,
                rectClient.Y + rectControl.Y,
                rectControl.Width,
                rectControl.Height);
        }

        public static void SelectListViewItem(IntPtr hwnd, IntPtr listView, string item)
        {
            string itemIdStr = AutoItX.ControlListView(hwnd, listView, "FindItem", item, "");

            bool succeeded = false;

            if (!string.IsNullOrEmpty(itemIdStr))
            {
                int itemId;
                if (int.TryParse(itemIdStr, out itemId))
                {
                    if (itemId >= 0)
                    {
                        AutoItX.ControlListView(hwnd, listView, "Select", itemId.ToString(), "");
                        AutoItX.Sleep(500);

                        succeeded = true;
                    }
                }
            }

            if (!succeeded)
            {
                throw new InvalidOperationException(
                    string.Format("failed to find item id for {0}", item));
            }
        }
    }
}
