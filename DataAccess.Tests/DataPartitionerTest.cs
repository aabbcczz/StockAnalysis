using System.Collections.Generic;
using System;
using DataAccess;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccess.Tests
{
    /// <summary>This class contains parameterized unit tests for DataPartitioner</summary>
    [TestClass]
    [PexClass(typeof(DataPartitioner))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class DataPartitionerTest
    {

        /// <summary>Test stub for PartitionData(DataDescription, DateTime, DateTime)</summary>
        [PexMethod]
        [PexAllowedException(typeof(NotSupportedException))]
        public IEnumerable<DataPartitionDescription> PartitionDataTest(
            DataDescription dataDescription,
            DateTime startTimeInclusive,
            DateTime endTimeExclusive
        )
        {
            IEnumerable<DataPartitionDescription> result
               = DataPartitioner.PartitionData(dataDescription, startTimeInclusive, endTimeExclusive);
            return result;
            // TODO: add assertions to method DataPartitionerTest.PartitionDataTest(DataDescription, DateTime, DateTime)
        }
    }
}
