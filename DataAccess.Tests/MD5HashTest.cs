using System;
using DataAccess;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccess.Tests
{
    /// <summary>This class contains parameterized unit tests for MD5Hash</summary>
    [TestClass]
    [PexClass(typeof(MD5Hash))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class MD5HashTest
    {

        /// <summary>Test stub for GetHash(String)</summary>
        [PexMethod]
        public byte[] GetHashTest(string inputString)
        {
            byte[] result = MD5Hash.GetHash(inputString);
            return result;
            // TODO: add assertions to method MD5HashTest.GetHashTest(String)
        }

        /// <summary>Test stub for GetHashString(String)</summary>
        [PexMethod]
        public string GetHashStringTest(string inputString)
        {
            string result = MD5Hash.GetHashString(inputString);
            return result;
            // TODO: add assertions to method MD5HashTest.GetHashStringTest(String)
        }
    }
}
