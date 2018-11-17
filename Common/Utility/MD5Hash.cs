namespace StockAnalysis.Common.Utility
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class MD5Hash
    {
        public static byte[] GetHash(string inputString)
        {
            if (inputString == null)
            {
                throw new ArgumentNullException("inputString");
            }

            HashAlgorithm algorithm = MD5.Create();  //or use SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            if (inputString == null)
            {
                throw new ArgumentNullException("inputString");
            }

            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
