using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    internal sealed class CirculatedArray<T> 
        where T : struct
	{
        private T[] _storage;
        private int _startIndex;
        private int _endIndex;
        private int _capacity;

        public int Length { get { return _capacity; } }

        public T this[int index]
        {
            get 
            { 
                if (index < 0 || index >= _capacity)
                {
                    throw new IndexOutOfRangeException();
                }

                index = (_startIndex + index) % _capacity;
                return _storage[index]; 
            }
        }
		
        public CirculatedArray(int capacity)
        {
            _storage = new T[capacity];

            _capacity = capacity;

            _startIndex = 0;

            _endIndex = _storage.Length - 1;
        }

        public void Add(T value)
        {
            _startIndex = (_startIndex + 1) % _capacity;
            _endIndex = (_endIndex + 1) % _capacity;

            _storage[_endIndex] = value;
        }
	}
}
