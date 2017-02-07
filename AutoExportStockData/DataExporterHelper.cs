using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using AutoIt;

namespace AutoExportStockData
{
    static class DataExporterHelper
    {
        public static IntPtr CleanUpAndGetMainWindowHandle()
        {
            AutoItX.Sleep(3000);
            AutoItX.WinClose("消息标题");
            //            AutoItX.WinClose("消息标题:交易提示");

            string title = "[TITLE:中信证券金融终端; CLASS:TdxW_MainFrame_Class]";

            int handle = AutoItX.WinWait(title, "", 120);

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
    }
}
