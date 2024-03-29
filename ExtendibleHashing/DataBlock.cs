﻿using ExtendibleHashing.DataInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ExtendibleHashing
{
    class DataBlock<T> : IBinarySerializable, IEnumerable<T> where T : IData, new()
    {
        private const int ValidItemsCountByteSize = sizeof(int); // 4

        private readonly int _blockByteSize;
        private readonly int _itemByteSize;
        private readonly int _maxItemCount;
        private readonly List<T> _items = new List<T>();

        public int MaxItemCount => _maxItemCount;

        public int Index { get; set; }
        public int InFileAddress { get; set; }
        public int BitDepth { get; set; }

        public int ItemCount => _items.Count;
        public bool IsFull => _items.Count >= _maxItemCount;

        public DataBlock(int index, int inFileAddress, int bitDepth, int blockByteSize)
        {
            Index = index;
            InFileAddress = inFileAddress;
            BitDepth = bitDepth;
            _blockByteSize = blockByteSize;

            T defaultItem = (T)Activator.CreateInstance(typeof(T));
            _itemByteSize = defaultItem.ByteSize;
            _maxItemCount = (_blockByteSize - ValidItemsCountByteSize) / _itemByteSize;
        }

        public DataBlock(int index, int inFileAddress, int bitDepth, byte[] data)
            : this(index, inFileAddress, bitDepth, data.Length)
        {
            FromByteArray(data, 0);
        }

        /// <summary>
        /// Creates DataBlock with <paramref name="data"/> and with invalid properties.
        /// </summary>
        /// <param name="data"></param>
        public DataBlock(byte[] data) : this(-1, -1, -1, data) { }

        /// <summary>
        /// Adds item to the block.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (Contains(item))
                throw new ArgumentException("This item is already present in the file.");

            if (!IsFull)
            {
                _items.Add(item);
            }
            else throw new Exception("DataBlock is full, you cannot add more items.");
        }

        /// <summary>
        /// Finds item based on <paramref name="itemId"/>.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public T Find(T itemId)
        {
            List<T> foundItems = _items.FindAll(i => i.IdEquals(itemId));
            if (foundItems.Count == 1)
            {
                return foundItems[0];
            }
            else if (foundItems.Count == 0)
            {
                return default;
            }
            throw new Exception("There is more items with the same IDs.");
        }

        /// <summary>
        /// True is there is present item with id of <paramref name="itemId"/>.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool Contains(T itemId)
        {
            return Find(itemId) != null;
        }

        /// <summary>
        /// Removes from block.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns>Success</returns>
        public bool Remove(T itemId)
        {
            if (Contains(itemId))
            {
                _items.RemoveAll(i => i.IdEquals(itemId));
                return true;
            }
            return false;
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

        /// <summary>
        /// Updates <paramref name="newItem"/> if exists in the block.
        /// </summary>
        /// <param name="newItem"></param>
        /// <returns>Success</returns>
        internal bool Update(T newItem)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].IdEquals(newItem))
                {
                    _items[i] = newItem;
                    return true;
                }
            }
            return false;
        }

    }
}
