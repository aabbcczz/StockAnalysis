using StockAnalysis.Share;
using System;
using DataAccess;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccess.Tests
{
    /// <summary>This class contains parameterized unit tests for FileStorageAccessorHelper</summary>
    [TestClass]
    [PexClass(typeof(FileStorageAccessorHelper))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class FileStorageAccessorHelperTest
    {

        /// <summary>Test stub for ConvertStringToValidFileName(String)</summary>
        [PexMethod]
        internal string ConvertStringToValidFileNameTest(string s)
        {
            string result = FileStorageAccessorHelper.ConvertStringToValidFileName(s);
            return result;
            // TODO: add assertions to method FileStorageAccessorHelperTest.ConvertStringToValidFileNameTest(String)
        }

        /// <summary>Test stub for ConvertStringToValidPath(String)</summary>
        [PexMethod]
        internal string ConvertStringToValidPathTest(string s)
        {
            string result = FileStorageAccessorHelper.ConvertStringToValidPath(s);
            return result;
            // TODO: add assertions to method FileStorageAccessorHelperTest.ConvertStringToValidPathTest(String)
        }

        /// <summary>Test stub for GetGranularityString(UInt32)</summary>
        [PexMethod]
        internal string GetGranularityStringTest(uint granularity)
        {
            string result = FileStorageAccessorHelper.GetGranularityString(granularity);
            return result;
            // TODO: add assertions to method FileStorageAccessorHelperTest.GetGranularityStringTest(UInt32)
        }

        /// <summary>Test stub for GetPartitionDataAbsolutePath(String, DataPartitionDescription, SecuritySymbol)</summary>
        [PexMethod]
        internal string GetPartitionDataAbsolutePathTest(
            string rootPath,
            DataPartitionDescription description,
            SecuritySymbol symbol
        )
        {
            string result
               = FileStorageAccessorHelper.GetPartitionDataAbsolutePath(rootPath, description, symbol);
            return result;
            // TODO: add assertions to method FileStorageAccessorHelperTest.GetPartitionDataAbsolutePathTest(String, DataPartitionDescription, SecuritySymbol)
        }

        /// <summary>Test stub for GetPartitionDataFileName(DataPartitionDescription, SecuritySymbol)</summary>
        [PexMethod]
        internal string GetPartitionDataFileNameTest(DataPartitionDescription description, SecuritySymbol symbol)
        {
            string result = FileStorageAccessorHelper.GetPartitionDataFileName(description, symbol);
            return result;
            // TODO: add assertions to method FileStorageAccessorHelperTest.GetPartitionDataFileNameTest(DataPartitionDescription, SecuritySymbol)
        }

        /// <summary>Test stub for GetPartitionDataRelativePath(DataPartitionDescription, SecuritySymbol)</summary>
        [PexMethod]
        internal string GetPartitionDataRelativePathTest(DataPartitionDescription description, SecuritySymbol symbol)
        {
            string result = FileStorageAccessorHelper.GetPartitionDataRelativePath(description, symbol);
            return result;
            // TODO: add assertions to method FileStorageAccessorHelperTest.GetPartitionDataRelativePathTest(DataPartitionDescription, SecuritySymbol)
        }

        /// <summary>Test stub for GetSymbolHashPrefix(String, Int32)</summary>
        [PexMethod]
        internal string GetSymbolHashPrefixTest(string symbol, int prefixLength)
        {
            string result = FileStorageAccessorHelper.GetSymbolHashPrefix(symbol, prefixLength);
            return result;
            // TODO: add assertions to method FileStorageAccessorHelperTest.GetSymbolHashPrefixTest(String, Int32)
        }
    }
}
