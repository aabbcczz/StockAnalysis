using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace RealTrading
{
    public sealed class TdxWrapper
    {
        ///基本版
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void OpenTdx();
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void CloseTdx();
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern int Logon(string IP, short Port, string Version, short YybID, string AccountNo, string TradeAccount, string JyPassword, string TxPassword, StringBuilder ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void Logoff(int ClientID);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void QueryData(int ClientID, int Category, StringBuilder Result, StringBuilder ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void SendOrder(int ClientID, int Category, int PriceType, string Gddm, string Zqdm, float Price, int Quantity, StringBuilder Result, StringBuilder ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void CancelOrder(int ClientID, string ExchangeID, string hth, StringBuilder Result, StringBuilder ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void GetQuote(int ClientID, string Zqdm, StringBuilder Result, StringBuilder ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void Repay(int ClientID, string Amount, StringBuilder Result, StringBuilder ErrInfo);




        ///普通批量版
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void QueryHistoryData(int ClientID, int Category, string StartDate, string EndDate, StringBuilder Result, StringBuilder ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void QueryDatas(int ClientID, int[] Category, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void SendOrders(int ClientID, int[] Category, int[] PriceType, string[] Gddm, string[] Zqdm, float[] Price, int[] Quantity, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void CancelOrders(int ClientID, string[] ExchangeID, string[] hth, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void GetQuotes(int ClientID, string[] Zqdm, int Count, IntPtr[] Result, IntPtr[] ErrInfo);


        ///高级批量版
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void QueryMultiAccountsDatas(int[] ClientID, int[] Category, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void SendMultiAccountsOrders(int[] ClientID, int[] Category, int[] PriceType, string[] Gddm, string[] Zqdm, float[] Price, int[] Quantity, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void CancelMultiAccountsOrders(int[] ClientID, string[] ExchangeID, string[] hth, int Count, IntPtr[] Result, IntPtr[] ErrInfo);
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void GetMultiAccountsQuotes(int[] ClientID, string[] Zqdm, int Count, IntPtr[] Result, IntPtr[] ErrInfo);


        public static string DecryptAccount(string encryptedAccount)
        {
            int len = encryptedAccount.Length;

            if (len % 2 != 0)
            {
                throw new ArgumentException("encryptedAccount");
            }

            len /= 2;

            ushort[] temp = new ushort[len];

            for (int i = 0; i < len; ++i)
            {
	            temp[i] = (ushort)((encryptedAccount[2 * i] - 'A') * 26 + (encryptedAccount[2*i + 1] - 'A'));
            }

            ushort key = 0x55E;
            for (int i = 0; i < len; ++i)
            {
	            ushort v = temp[i];
	
	            temp[i] = (ushort)((key >> 8) ^ v);
	            key = (ushort)((((uint)v + (uint)key) * 0x5207F + 0x6adc3) & 0xFFFF);
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < temp.Length; ++i)
            {
                builder.Append((char)temp[i]);
            }

            return builder.ToString();
        }

        public static string EncryptAccount(string account)
        {
            if (string.IsNullOrEmpty(account))
            {
                throw new ArgumentNullException("account");
            }

            int len = account.Length;

            // get the characters in odd pos
            len /= 2;

            string oddPosAccount = string.Empty;

            for (int i = 0; i < len; ++i)
            {
                oddPosAccount += account[2 * i];
            }
            
            if (account.Length % 2 != 0)
            {
                ++len;
                oddPosAccount += account[2 * len - 2];
            }

            ushort[] temp = new ushort[len];

            ushort key = 0x55E;
            for (int i = 0; i < len; ++i)
            {
                ushort v = (ushort)oddPosAccount[i];

                temp[i] = (ushort)((key >> 8) ^ v);
                key = (ushort)((((uint)temp[i] + (uint)key) * 0x5207F + 0x6adc3) & 0xFFFF);
            }

            byte[] encryptedAccount = new byte[len * 2];

            for (int i = 0; i < len; ++i)
            {
                byte a = (byte)(temp[i] / 26);
                byte b = (byte)(temp[i] % 26);

                encryptedAccount[2 * i] = (byte)(a + 'A');
                encryptedAccount[2 * i + 1] = (byte)(b + 'A');
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < encryptedAccount.Length; ++i)
            {
                builder.Append((char)encryptedAccount[i]);
            }

            return builder.ToString();
        }
    }
}
