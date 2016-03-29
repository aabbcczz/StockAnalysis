namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    internal class MessageStatistics
    {
        private int _numberOfType;
        private long[] _countOfMessages;
        private long[] _sizeOfMessages;

        public MessageStatistics()
        {
            _numberOfType = (int)MessageType.TypeCount;
            _countOfMessages = new long[_numberOfType];
            _sizeOfMessages = new long[_numberOfType];
        }

        public void Reset()
        {
            for (int i = 0; i < _numberOfType; i++)
            {
                _countOfMessages[i] = 0;
                _sizeOfMessages[i] = 0;
            }
        }

        public void AddMessage(MessageType type, int size)
        {
            Interlocked.Increment(ref _countOfMessages[(int)type]);
            Interlocked.Add(ref _sizeOfMessages[(int)type], (long)size);
        }

        public string GetStatistics()
        {
            StringBuilder statistics = new StringBuilder();

            for (int i = 0; i < _numberOfType; i++)
            {
                if (_countOfMessages[i] > 0)
                {
                    statistics.AppendLine(string.Format(
                        "{0} ({1} {2:f4}MB)",
                        ((MessageType)i).ToString(),
                        _countOfMessages[i],
                        (double)_sizeOfMessages[i] / 1024.0 / 1024.0));
                }
            }

            return statistics.ToString();
        }
    }
}
