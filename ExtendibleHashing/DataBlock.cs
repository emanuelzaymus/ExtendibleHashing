using System;
using System.Collections.Generic;

namespace ExtendibleHashing
{
    class DataBlock<T> : IBinarySerializable where T : IBinarySerializable
    {
        private readonly int _blockByteSize;
        private readonly int _maxCount;

        public int Count { get; private set; } = 0;
        public List<T> Items { get; } = new List<T>(1);

        public DataBlock(int blockByteSize)
        {
            _blockByteSize = blockByteSize;
            T defaultItem = (T)Activator.CreateInstance(typeof(T));
            _maxCount = _blockByteSize / defaultItem.ByteSize;
        }

        public DataBlock(byte[] data) : this(data.Length)
        {
            FromByteArray(data, 0);
        }

        public bool IsFull => Count >= _maxCount;

        internal void Add(T item)
        {
            throw new NotImplementedException();
            Count++;
        }

        public int ByteSize => throw new NotImplementedException();

        public byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }

        public void FromByteArray(byte[] byteArray, int offset)
        {
            throw new NotImplementedException();
        }

    }
}
