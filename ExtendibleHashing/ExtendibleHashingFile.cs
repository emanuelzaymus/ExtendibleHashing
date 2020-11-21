﻿using ExtendibleHashing.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExtendibleHashing
{
    public class ExtendibleHashingFile<T> : IDisposable where T : IBinarySerializable
    {
        private readonly FileStream _file;
        private readonly FileStream _overfillFile;
        private readonly string _managerFilePath;
        private readonly int _blockByteSize;

        private readonly List<int> _dataBlockPositions = new List<int>() { 0 };
        private int _numberOfRelevantBits = 0;

        //private DataBlock<T> _actualBlock;
        //private DataBlock<T> _hlpBlock;

        /// <summary>
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="overfillingFilePath"></param>
        /// <param name="managerFilePath"></param>
        /// <param name="blockByteSize">Block size in bytes</param>
        public ExtendibleHashingFile(string filePath, string overfillingFilePath, string managerFilePath, int blockByteSize = 4096)
        {
            _file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _overfillFile = new FileStream(overfillingFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _managerFilePath = managerFilePath;
            _blockByteSize = blockByteSize;
            // Load initializing data from _managerFilePath

        }

        public void Add(T item)
        {
            var block = GetDataBlock(item);

            if (block.IsFull)
            {
                throw new Exception();
            }
            else
            {
                block.Add(item);
                Save(block); // Maybe I dont need to save every time - only when I'm changing loaded block.
            }
        }

        public T Find(T itemPosition)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T itemPosition)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _file.Dispose();
            _overfillFile.Dispose();
            // Write everithing necessary into _manageFilePath
        }

        private DataBlock<T> GetDataBlock(T itemPosition)
        {
            BitArray bits = HashCodeToBitArray(itemPosition.GetHashCode());
            int index = bits.IntFromFirst(_numberOfRelevantBits);

            int position = _dataBlockPositions[index];

            byte[] data = ReadBlock(position);
            DataBlock<T> ret = new DataBlock<T>(_blockByteSize);
            ret.FromByteArray(data, 0);
            throw new NotImplementedException();
        }

        private byte[] ReadBlock(int position)
        {
            _file.Seek(position, SeekOrigin.Begin);
            byte[] block = new byte[_blockByteSize];
            _file.Read(block, position, _blockByteSize);
            return block;
        }

        private BitArray HashCodeToBitArray(int hashCode)
        {
            return new BitArray(BitConverter.GetBytes(hashCode));
        }

        private void Save(DataBlock<T> block)
        {
            throw new NotImplementedException();
        }

    }
}
