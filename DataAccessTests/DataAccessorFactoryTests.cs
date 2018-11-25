using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalysis.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using StockAnalysis.Common.Data;
using System.IO;
using System.Diagnostics;
using StockAnalysis.Common.SymbolName;
using StockAnalysis.Common.Exchange;
using TeaTime;

namespace StockAnalysis.DataAccess.Tests
{
    [TestClass()]
    public class DataAccessorFactoryTests
    {
        [TestMethod()]
        public void CreateFileStorageAccessorTest()
        {
            var rootPath = GetFileStorageRootPath();
            IDataAccessor accessor = DataAccessorFactory.CreateFileStorageAccessor(rootPath);

            accessor.Should().NotBeNull();
        }

        [TestMethod()]
        public void FileStorageAccessorTest()
        {
            var rootPath = GetFileStorageRootPath();
            
            // clean up all existing files to avoid impact to tests.
            CleanFiles(rootPath);

            IDataAccessor accessor = DataAccessorFactory.CreateFileStorageAccessor(rootPath);

            accessor.Should().NotBeNull();

            TestDataAccessForGranularity(accessor, DataGranularity.D1y);
            TestDataAccessForGranularity(accessor, DataGranularity.D1season);
            TestDataAccessForGranularity(accessor, DataGranularity.D1w);
            TestDataAccessForGranularity(accessor, DataGranularity.D1d);
            TestDataAccessForGranularity(accessor, DataGranularity.D4h);
            TestDataAccessForGranularity(accessor, DataGranularity.D3h);
            TestDataAccessForGranularity(accessor, DataGranularity.D2h);
            TestDataAccessForGranularity(accessor, DataGranularity.D1h);
            TestDataAccessForGranularity(accessor, DataGranularity.D30min);
            TestDataAccessForGranularity(accessor, DataGranularity.D15min);
            TestDataAccessForGranularity(accessor, DataGranularity.D10min);
            TestDataAccessForGranularity(accessor, DataGranularity.D5min);
            TestDataAccessForGranularity(accessor, DataGranularity.D3min);
            TestDataAccessForGranularity(accessor, DataGranularity.D1min);
            TestDataAccessForGranularity(accessor, DataGranularity.D30s);
            TestDataAccessForGranularity(accessor, DataGranularity.D15s);
            TestDataAccessForGranularity(accessor, DataGranularity.D10s);
            TestDataAccessForGranularity(accessor, DataGranularity.D5s);
            TestDataAccessForGranularity(accessor, DataGranularity.D1s);
        }

        private void TestDataAccessForGranularity(IDataAccessor accessor, DataGranularity granularity)
        {
            // prepare Bar data;
            var testDataToBeWritten = PrepareBarTestData(granularity);

            DataDescription description = new DataDescription()
            {
                Category = DataCategory.Stock,
                Granularity = (uint)granularity,
                RepricingRight = RepricingRight.ForwardRight,
                Schema = DataSchema.Bar
            };

            SecuritySymbol symbol = new SecuritySymbol("600001", "SH.600001", ExchangeId.ShanghaiSecurityExchange);

            // test write and read consistency
            accessor.WriteData(testDataToBeWritten, description, symbol);

            var startTimeInclusive = testDataToBeWritten[0].Time;
            var endTimeExclusive = testDataToBeWritten[testDataToBeWritten.Count - 1].Time.AddTicks(1);
            var readoutDataEnumerable = accessor.ReadData<Bar>(description, symbol, startTimeInclusive, endTimeExclusive);
            var readoutData = readoutDataEnumerable.ToList();
            CompareData(testDataToBeWritten, readoutData).Should().BeTrue();

            // test overwriting functionality
            accessor.WriteData(testDataToBeWritten, description, symbol);
            var readoutData1 = accessor.ReadData<Bar>(description, symbol, startTimeInclusive, endTimeExclusive).ToList();
            CompareData(testDataToBeWritten, readoutData1).Should().BeTrue();

            // test partial overwritting consistency
            int skipCount = testDataToBeWritten.Count / 3;
            int takeCount = testDataToBeWritten.Count / 3;

            var testData2 = testDataToBeWritten.Skip(skipCount).Take(takeCount).ToList();
            accessor.WriteData(testData2, description, symbol);
            startTimeInclusive = testData2.First().Time;
            endTimeExclusive = testData2.Last().Time.AddTicks(1);
            var readoutData2 = accessor.ReadData<Bar>(description, symbol, startTimeInclusive, endTimeExclusive).ToList();
            CompareData(testDataToBeWritten, readoutData1).Should().BeTrue();
        }

        private List<Bar> PrepareBarTestData(DataGranularity granularity)
        {
            Random rand = new Random();

            int dayCount = 800; // about 2~3 years.
            int interval = (int)granularity;
            TimeSpan dayStartTime = new TimeSpan(9, 30, 0);
            TimeSpan dayEndTime = new TimeSpan(15, 0, 0);
            int barCountPerDay = (int)(dayEndTime - dayStartTime).TotalSeconds / interval;
            if (barCountPerDay == 0)
            {
                barCountPerDay = 1;
            }

            int dataSize = barCountPerDay * dayCount;

            dataSize = Math.Min(dataSize, 200 * 1000);

            List<Bar> testData = new List<Bar>(dataSize);

            DateTime startTime = new DateTime(1999, 1, 1) + dayStartTime;

            for (int i = 0; i < dataSize; i++)
            {
                double openPrice = 10.0 + Math.Round(rand.NextDouble() * 100.0, 2) / 100.0;

                testData.Add(
                    new Bar()
                    {
                        TeaTime = startTime,
                        OpenPrice = openPrice,
                        ClosePrice = openPrice + 1.0,
                        HighestPrice = openPrice + 1.5,
                        LowestPrice = openPrice - 0.5,
                        Amount = rand.NextDouble() * 1000000,
                        Volume = 1203948200,
                        OpenInterest = 0,
                    });

                startTime = startTime.AddSeconds(interval);
                if (startTime.TimeOfDay >= dayEndTime)
                {
                    startTime = startTime.AddDays(1.0).Date + dayStartTime;
                }

                if (startTime >= DataPartitioner.MaxPartitionTimeExclusive)
                {
                    break;
                }
            }

            return testData;
        }

        private bool CompareData(List<Bar> testDataToBeWritten, List<Bar> readoutData)
        {
            if (testDataToBeWritten.Count() != readoutData.Count())
            {
                return false;
            }

            for (int i = 0; i < testDataToBeWritten.Count; ++i)
            {
                Bar t1 = testDataToBeWritten[i];
                Bar t2 = readoutData[i];

                if (t1.Time != t2.Time 
                    || t1.OpenInterest != t2.OpenInterest
                    || t1.OpenPrice != t2.OpenPrice
                    || t1.ClosePrice != t2.ClosePrice
                    || t1.HighestPrice != t2.HighestPrice
                    || t1.LowestPrice != t2.LowestPrice
                    || t1.Amount != t2.Amount
                    || t1.Volume != t2.Volume)
                {
                    return false;
                }
            }

            return true;
        }

        private string GetFileStorageRootPath()
        {
            var rootPath = Path.Combine(Path.GetTempPath(), "TestFileStorageAccessor");

            return rootPath;
        }

        private void CleanFiles(string rootPath)
        {
            if (!Directory.Exists(rootPath))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(rootPath))
            {
                File.Delete(file);
            }

            foreach (var subDirectory in Directory.GetDirectories(rootPath))
            {
                CleanFiles(subDirectory);
            }

            Directory.Delete(rootPath);
        }
    }
}