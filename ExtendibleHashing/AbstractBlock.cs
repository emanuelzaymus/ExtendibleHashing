using ExtendibleHashing.DataInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ExtendibleHashing
{
    abstract class AbstractBlock<T> : IBinarySerializable, IEnumerable<T> where T : IData, new()
    {
        private const int ValidItemsCountByteSize = sizeof(int);

        private readonly int _blockByteSize;
        private readonly int _itemByteSize;
        protected readonly List<T> _items = new List<T>();

        public int Address { get; }

        protected int MaxItemCount { get; }
        public bool IsFull => _items.Count >= MaxItemCount;

        public AbstractBlock(int address, int blockByteSize)
        {
            Address = address;
            _blockByteSize = blockByteSize;

            T defaultItem = (T)Activator.CreateInstance(typeof(T));
            _itemByteSize = defaultItem.ByteSize;
            MaxItemCount = (_blockByteSize - ValidItemsCountByteSize) / _itemByteSize;
        }

        public int ByteSize => _blockByteSize;

        public byte[] ToByteArray()
        {
            var ret = new byte[_blockByteSize];
            // Firstly add number of valid items.
            Array.Copy(BitConverter.GetBytes(_items.Count), 0, ret, 0, ValidItemsCountByteSize);
            for (int i = 0; i < _items.Count; i++)
            {
                T item = _items[i];
                // Add items to the array one by one.
                Array.Copy(item.ToByteArray(), 0, ret, ValidItemsCountByteSize + _itemByteSize * i, _itemByteSize);
            }
            return ret;
        }

        public void FromByteArray(byte[] byteArray, int offset)
        {
            _items.Clear();
            int validItemsCount = BitConverter.ToInt32(byteArray, offset);
            for (int i = 0; i < validItemsCount; i++)
            {
                // Create instance of T
                T item = (T)Activator.CreateInstance(typeof(T));
                // Initialize new instance with byte array
                item.FromByteArray(byteArray, offset + ValidItemsCountByteSize + _itemByteSize * i);
                _items.Add(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
