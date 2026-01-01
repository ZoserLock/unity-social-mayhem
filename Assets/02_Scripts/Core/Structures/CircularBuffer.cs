using System;

namespace Zen.Core
{
    // Implementation of a simple circular buffer. 
    // A circular buffer is a data structure that uses a single, fixed-size buffer as if it were connected end-to-end.
    public class CircularBuffer<T>
    {
        private T[] _buffer;
        
        private int _start;
        private int _end;
        private int _size;

        public CircularBuffer(int capacity)
            : this(capacity, new T[] { })
        {

        }

        public CircularBuffer(int capacity, T[] items)
        {
            if (capacity < 1)
            {
                throw new ArgumentException(
                    "Circular buffer cannot have negative or zero capacity.", "capacity");
            }
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            if (items.Length > capacity)
            {
                throw new ArgumentException(
                    "Too many items to fit circular buffer", "items");
            }

            _buffer = new T[capacity];

            Array.Copy(items, _buffer, items.Length);
            _size = items.Length;

            _start = 0;
            _end = _size == capacity ? 0 : _size;
        }

        /// <summary>
        /// Maximum capacity of the buffer. Elements pushed into the buffer after
        /// maximum capacity is reached (IsFull = true), will remove an element.
        /// </summary>
        public int Capacity { get { return _buffer.Length; } }

        public bool IsFull
        {
            get
            {
                return Count == Capacity;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }

        /// <summary>
        /// Current buffer size (the number of elements that the buffer has).
        /// </summary>
        public int Count { get { return _size; } }

        /// <summary>
        /// Element at the front of the buffer - this[0].
        /// </summary>
        /// <returns>The value of the element of type T at the front of the buffer.</returns>
        public T Front()
        {
            if (IsEmpty)
            {
                return default(T);
            }
            return _buffer[_start];
        }

        /// <summary>
        /// Element at the back of the buffer - this[Size - 1].
        /// </summary>
        /// <returns>The value of the element of type T at the back of the buffer.</returns>
        public T Back()
        {
            if (IsEmpty)
            {
                return default(T);
            }

            return _buffer[(_end != 0 ? _end : _size) - 1];
        }

        public T this[int index]
        {
            get
            {
                if (IsEmpty)
                {
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer is empty", index));
                }
                if (index >= _size)
                {
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer size is {1}", index, _size));
                }
                int actualIndex = InternalIndex(index);
                return _buffer[actualIndex];
            }
            set
            {
                if (IsEmpty)
                {
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer is empty", index));
                }
                if (index >= _size)
                {
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer size is {1}", index, _size));
                }
                int actualIndex = InternalIndex(index);
                _buffer[actualIndex] = value;
            }
        }

        public void PushBack(T item)
        {
            if (IsFull)
            {
                _buffer[_end] = item;
                Increment(ref _end);
                _start = _end;
            }
            else
            {
                _buffer[_end] = item;
                Increment(ref _end);
                ++_size;
            }
        }

        /// <summary>
        /// Pushes a new element to the front of the buffer. Front()/this[0]
        /// will now return this element.
        /// 
        /// When the buffer is full, the element at Back()/this[Size-1] will be 
        /// popped to allow for this new element to fit.
        /// </summary>
        public void PushFront(T item)
        {
            if (IsFull)
            {
                Decrement(ref _start);
                _end = _start;
                _buffer[_start] = item;
            }
            else
            {
                Decrement(ref _start);
                _buffer[_start] = item;
                ++_size;
            }
        }

        /// <summary>
        /// Removes the element at the back of the buffer. Decreassing the 
        /// Buffer size by 1.
        /// </summary>
        public void PopBack()
        {
            if (!IsEmpty)
            {
                Decrement(ref _end);
                _buffer[_end] = default(T);
                --_size;
            }
        }

        /// <summary>
        /// Removes the element at the front of the buffer. Decreassing the 
        /// Buffer size by 1.
        /// </summary>
        public void PopFront()
        {
            if (!IsEmpty)
            {
                _buffer[_start] = default(T);
                Increment(ref _start);
                --_size;
            }
        }

        /// <summary>
        /// Increments the provided index variable by one, wrapping
        /// around if necessary.
        /// </summary>
        /// <param name="index"></param>
        private void Increment(ref int index)
        {
            if (++index == Capacity)
            {
                index = 0;
            }
        }

        /// <summary>
        /// Decrements the provided index variable by one, wrapping
        /// around if necessary.
        /// </summary>
        /// <param name="index"></param>
        private void Decrement(ref int index)
        {
            if (index == 0)
            {
                index = Capacity;
            }
            index--;
        }

        /// <summary>
        /// Converts the index in the argument to an index in <code>_buffer</code>
        /// </summary>
        private int InternalIndex(int index)
        {
            return _start + (index < (Capacity - _start) ? index : index - Capacity);
        }
    }
}