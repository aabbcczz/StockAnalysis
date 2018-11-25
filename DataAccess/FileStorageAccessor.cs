namespace StockAnalysis.DataAccess
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Common.Data;
    using Common.SymbolName;

    using TeaTime;

    internal class FileStorageAccessor : IDataAccessor
    {
        public string StorageRootPath { get; private set; }

        private const int maxRetryTimes = 3;
        private const int initialBackoffTimeInMS = 500;

        private Tuple<bool, TResult> ExecuteWithRetry<TResult>(Func<TResult> function)
        {
            int retryTimes = 0;
            int backoffTimeInMS = initialBackoffTimeInMS;

            while (retryTimes <= maxRetryTimes)
            {
                try
                {
                    TResult result = function();
                    return Tuple.Create(true, result);
                }
                catch (Exception)
                {
                    retryTimes++;
                    if (retryTimes > maxRetryTimes)
                    {
                        throw;
                    }

                    // back off by sleeping
                    Thread.Sleep(backoffTimeInMS);

                    // double backoff time in each retry.
                    backoffTimeInMS *= 2;
                }
            }

            return Tuple.Create(false, default(TResult));
        }

        public FileStorageAccessor(string storageRootPath)
        {
            if (string.IsNullOrWhiteSpace(storageRootPath))
            {
                throw new ArgumentNullException();
            }

            if (!Path.IsPathRooted(storageRootPath))
            {
                throw new ArgumentException($"{storageRootPath} is not rooted path");
            }

            StorageRootPath = storageRootPath;
        }

        public IEnumerable<T> ReadData<T>(DataDescription description, SecuritySymbol symbol, DateTime startTimeInclusive, DateTime endTimeExclusive)
            where T : struct, ITimeSeriesData
        {
            if (description == null || symbol == null)
            {
                throw new ArgumentNullException();
            }

            if (startTimeInclusive > endTimeExclusive)
            {
                throw new ArgumentException("start time should be earlier than end time");
            }

            // get data partitions for the data
            var partitions = DataPartitioner.PartitionData(description, startTimeInclusive, endTimeExclusive).ToArray();
            List<List<T>> results = new List<List<T>>(partitions.Length);

            for (int i = 0; i < partitions.Length; ++i)
            {
                // read data from each partition consecutively
                string fileName = FileStorageAccessorHelper.GetPartitionDataAbsolutePathAndFileName(StorageRootPath, partitions[i], symbol);

                if (!File.Exists(fileName))
                {
                    // file does not exist, so no data can be read out
                    continue;
                }

                var executeResult = ExecuteWithRetry(() => TeaFile<T>.OpenRead(fileName)); 
                if (!executeResult.Item1)
                {
                    // open read failed even with retry
                    continue;
                }

                using (var teaFile = executeResult.Item2)
                {
                    if (teaFile.Count == 0)
                    {
                        // no data in file, just skip it.
                        continue;
                    }

                    // skip unnecessary data from first partition
                    var begin = i == 0 
                        ? teaFile.Items.SkipWhile(t => t.Time < startTimeInclusive) 
                        : teaFile.Items;

                    // ignore unnecessary data at the end of last partition.
                    if (i == partitions.Length - 1)
                    {
                        results.Add(begin.TakeWhile(t => t.Time < endTimeExclusive).ToList());
                    }
                    else
                    {
                        results.Add(begin.ToList());
                    }
                }
            }

            // convert List<List<T>> to List<T>
            return results.SelectMany(list => list);
        }

        public void WriteData<T>(IEnumerable<T> data, DataDescription description, SecuritySymbol symbol)
            where T : struct, ITimeSeriesData
        {
            if (data == null | description == null)
            {
                throw new ArgumentNullException();
            }

            if (data.Count() == 0)
            {
                return;
            }
#if DEBUG
            // verify if data is ordered by time incrementally in DEBUG state
            var arrayData = data.ToArray();
            for (int i = 0; i < arrayData.Length - 1; i++)
            {
                if (arrayData[i].Time >= arrayData[i+1].Time)
                {
                    throw new InvalidDataException($"data is not ordered by time incrementally. Data[{i}]:{arrayData[i].Time}, Data[{i + 1}]:{arrayData[i + 1].Time}");
                }
            }
#endif
            // We always assume data has been ordered by time from older to newer. 
            // It is caller's responsiblity to ensure the order of data. 
            DateTime startTimeInclusive = data.First().Time;
            DateTime endTimeExclusive = data.Last().Time.AddTicks(1);

            // Get all partitions to be written
            var partitions = DataPartitioner.PartitionData(description, startTimeInclusive, endTimeExclusive);

            // prepare data enumerator for reading
            var enumerator = data.GetEnumerator();
            enumerator.MoveNext();

            foreach (var partition in partitions)
            {
                // get data to be stored in the partition.
                List<T> partitionData = new List<T>(1024);

                do
                {
                    T current = enumerator.Current;

                    if (current.Time < partition.StartTimeInclusive)
                    {
                        // error, the input data are not ordered
                        throw new InvalidDataException($"input data is not ordered, time of wrong data is {current.Time:O}");
                    }

                    if (current.Time < partition.EndTimeExclusive)
                    {
                        partitionData.Add(current);
                    }
                    else
                    {
                        break;
                    }
                } while (enumerator.MoveNext());

                if (partitionData.Count() == 0)
                {
                    // not data, so just skip it.
                    continue;
                }

                // get partition file name
                var fileName = FileStorageAccessorHelper.GetPartitionDataAbsolutePathAndFileName(StorageRootPath, partition, symbol);

                // try to open file for write or create a new file.
                var executeResult = ExecuteWithRetry(() =>
                    {
                        if (File.Exists(fileName))
                        {
                            return TeaFile<T>.OpenWrite(fileName);
                        }
                        else
                        {
                            string directory = Path.GetDirectoryName(fileName);
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            return TeaFile<T>.Create(fileName);
                        }
                    });

                if (!executeResult.Item1)
                {
                    // open file for writing or create new file failed even with retry
                    continue;
                }

                // merge data and write back
                using (var teaFile = executeResult.Item2)
                {
                    //
                    // if file is empty or the last record's time is earlier than first data item's time,
                    // we just append to the last.
                    //

                    //
                    // we don't use teaFile.Items.Last() to get the last item because it will read the whole
                    // file sequentially. instead, using Items[Items.Count - 1] needs only one read.
                    //
                    if (teaFile.Items.Count == 0 
                        || teaFile.Items[teaFile.Items.Count - 1].Time < partitionData.First().Time)
                    {
                        teaFile.SetFilePointerToEnd();
                        teaFile.Write(partitionData);
                    }
                    else
                    {
                        //
                        // we need to read file data out and merged with partition data, and then write out
                        // if an existing data item has the same time with one partition data item, partition 
                        // data item will overwrite existing data item.
                        //

                        // find out first item to be merged or overwritten.
                        int insertIndex = teaFile.Items.TakeWhile(t => t.Time < partitionData.First().Time).Count();

                        // read out all data to be overwritten in file 
                        var oldData = teaFile.Items.SkipWhile(t => t.Time < partitionData.First().Time).ToList();

                        // merge old data with partition data.
                        var mergedData = MergeAndOverwriteData(oldData, partitionData);

                        //
                        // write back merged data to file
                        //
                        teaFile.SetFilePointerToItem(insertIndex);
                        teaFile.Write(mergedData);
                    }
                }
            }
        }

        private List<T> MergeAndOverwriteData<T>(List<T> oldData, List<T> newData)
            where T : struct, ITimeSeriesData
        {
            if (oldData == null || newData == null)
            {
                throw new ArgumentNullException();
            }

            if (oldData.Count == 0)
            {
                return new List<T>(newData);
            }

            if (newData.Count == 0)
            {
                return new List<T>(oldData);
            }

            List<T> mergedData = new List<T>(oldData.Count() + newData.Count());
            int oldDataIndex = 0;
            int newDataIndex = 0;

            while (oldDataIndex < oldData.Count && newDataIndex < newData.Count)
            {
                Time timeOld = oldData[oldDataIndex].Time;
                Time timeNew = newData[newDataIndex].Time;

                if (timeOld < timeNew)
                {
                    mergedData.Add(oldData[oldDataIndex]);
                    oldDataIndex++;
                }
                else if (timeOld == timeNew)
                {
                    mergedData.Add(newData[newDataIndex]);

                    // old data is overwritten, so index should be increased.
                    oldDataIndex++;
                    newDataIndex++;
                }
                else
                {
                    mergedData.Add(newData[newDataIndex]);
                    newDataIndex++;
                }
            }

            if (oldDataIndex == oldData.Count)
            {
                mergedData.AddRange(newData.Skip(newDataIndex));
            }
            else if (newDataIndex == newData.Count)
            {
                mergedData.AddRange(oldData.Skip(oldDataIndex));
            }
            else
            {
                // impossible, must be logic error
                throw new InvalidOperationException();
            }

            return mergedData;
        }
    }
}
