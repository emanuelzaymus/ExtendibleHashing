using ExtendibleHashing.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ExtendibleHashing
{
    public class ExtendibleHashingFile<T> : IDisposable, IEnumerable<T> where T : IBinarySerializable, new()
    {
        private readonly FileStream _file;
        private readonly FileStream _overfillFile;
        private readonly string _managerFilePath;
        private readonly int _blockByteSize;

        private List<int> _blockAddresses = new List<int>() { 0 }; // adresar
        private List<int> _blockBitDepths = new List<int>() { 0 }; // hlbky blokov

        private List<bool> _fileBlockOccupacity = new List<bool>() { false }; // obsadenost blokov --> musi byt v Managery TODO !!!

        private int _fileBitDepth = 0; // hlbka suboru
        //private int _fileByteSize = 0;

        /// <summary>
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="overfillingFilePath"></param>
        /// <param name="managerFilePath"></param>
        /// <param name="blockByteSize">Block size in bytes</param>
        public ExtendibleHashingFile(string filePath, string overfillingFilePath, string managerFilePath, int blockByteSize = 4096)
        {
            //_file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite); // TODO change to FileMode.OpenOrCreate !
            _file = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, blockByteSize, FileOptions.WriteThrough);
            _overfillFile = new FileStream(overfillingFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _managerFilePath = managerFilePath;
            _blockByteSize = blockByteSize;
            // Load initializing data from _managerFilePath
        }

        public void Add(T item)
        {
            while (true)
            {
                var block = GetDataBlock(item);

                if (block.IsFull)
                {
                    if (_fileBitDepth == block.BitDepth)
                    {
                        DoubleTheFileSize();
                    }
                    Split(block);
                }
                else
                {
                    block.Add(item);
                    Save(block);
                    break;
                }
            }
        }

        public T Find(T itemAddress)
        {
            var block = GetDataBlock(itemAddress);
            return block.Find(itemAddress);
        }

        public bool Remove(T itemAddress)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _file.Dispose();
            _overfillFile.Dispose();
            // Write everithing necessary into _manageFilePath
        }

        private DataBlock<T> GetDataBlock(T itemAddress)
        {
            if (_file.Length == 0)
            {
                return GetNewBlock(0);
            }
            BitArray bits = HashCodeToBitArray(itemAddress.GetHashCode());
            int index = bits.IntFromFirst(_fileBitDepth);

            int address = _blockAddresses[index];

            byte[] data = ReadBlock(address);
            return new DataBlock<T>(index, address, _blockBitDepths[index], data);
        }

        private DataBlock<T> GetNewBlock(int index)
        {
            int indexOfEmptyBlock = _fileBlockOccupacity.IndexOf(false);
            _fileBlockOccupacity[indexOfEmptyBlock] = true;
            if (indexOfEmptyBlock + 1 == _fileBlockOccupacity.Count)
            {
                _fileBlockOccupacity.Add(false);
            }
            int newAddress = indexOfEmptyBlock * _blockByteSize;
            _blockAddresses[index] = newAddress;

            return new DataBlock<T>(index, newAddress, _blockBitDepths[index], _blockByteSize);
        }

        private byte[] ReadBlock(int address)
        {
            _file.Seek(address, SeekOrigin.Begin);
            byte[] block = new byte[_blockByteSize];
            _file.Read(block, 0, _blockByteSize);
            return block;
        }

        private BitArray HashCodeToBitArray(int hashCode)
        {
            return new BitArray(BitConverter.GetBytes(hashCode));
            //return new BitArray(BitConverter.GetBytes(hashCode)).And(new BitArray(new[] { true, true, true }));
        }

        private void DoubleTheFileSize()
        {
            _blockAddresses = _blockAddresses.DoubleValues();
            _blockBitDepths = _blockBitDepths.DoubleValues();
            _fileBitDepth++;
        }

        private void Split(DataBlock<T> block)
        {
            int index = block.Index;
            _blockBitDepths[index]++;
            _blockBitDepths[index + 1]++;

            DataBlock<T> block1 = new DataBlock<T>(block.Index, block.InFileAddress, _blockBitDepths[block.Index], _blockByteSize);
            DataBlock<T> block2 = GetNewBlock(index + 1);

            foreach (var item in block)
            {
                BitArray bits = HashCodeToBitArray(item.GetHashCode());
                int i = bits.IntFromFirst(_fileBitDepth);

                if (i == block1.Index)
                {
                    block1.Add(item);
                }
                else if (i == block2.Index)
                {
                    block2.Add(item);
                }
                else throw new Exception("You should not get here");
            }
            Save(block1); // TODO: write at onece
            Save(block2);
        }

        private void Save(DataBlock<T> block)
        {
            _file.Seek(block.InFileAddress, SeekOrigin.Begin);
            _file.Write(block.ToByteArray(), 0, block.ByteSize);
            _file.Flush(true);
        }

        public IEnumerator<T> GetEnumerator()
        {
            int lastAddr = -1;
            foreach (var addr in _blockAddresses)
            {
                if (addr != lastAddr)
                {
                    lastAddr = addr;

                    byte[] bytes = ReadBlock(addr);
                    var block = new DataBlock<T>(bytes);
                    foreach (var item in block)
                    {
                        yield return item;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
