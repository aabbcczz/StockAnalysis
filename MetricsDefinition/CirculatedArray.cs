using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalysis.MetricsDefinition
{
    public sealed class CirculatedArray<T> 
        where T : struct
	{
        private readonly T[] _storage;
        private int _startIndex;
        private int _endIndex;
        private readonly int _capacity;
        private int _length;

        public int Length { get { return _length; } }

        public IEnumerable<T> InternalData
        {
            get { return _storage; }
        }

        public T this[int index]
        {
            get 
            {
                unchecked
                {
                    if (index < -_length || index >= _length)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    if (index < 0)
                    {
                        index += _length;
                    }

                    index = (_startIndex + index) % _length;
                    return _storage[index];
                }
            }
        }
		
        public CirculatedArray(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException("capacity should be greater than zero");
            }

            _storage = new T[capacity];
            _capacity = capacity;
            _startIndex = 0;
            _endIndex = 0;
            _length = 0;
        }

        public void Add(T value)
        {
            unchecked
            {
                // special case for adding first value.
                if (_length == 0)
                {
                    _storage[0] = value;
                    _endIndex = 0;
                    _length = 1;

                    return;
                }

                if (_length < _capacity)
                {
                    ++_endIndex;
                    ++_length;
                }
                else
                {
                    ++_startIndex;
                    ++_endIndex;

                    _startIndex %= _capacity;
                    _endIndex %= _capacity;
                }

                _storage[_endIndex] = value;
            }
        }
	}
}
