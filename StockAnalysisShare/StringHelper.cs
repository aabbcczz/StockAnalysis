using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public static class StringHelper
    {
        public static string Escape(this string s)
        {
            return s.Replace(",", "&comma;");
        }

        public static string Unescape(this string s)
        {
            return s.Replace("&comma;", ",");
        }
    }
}
