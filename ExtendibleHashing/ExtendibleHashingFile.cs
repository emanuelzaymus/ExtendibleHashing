using ExtendibleHashing.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ExtendibleHashing
{
    public class ExtendibleHashingFile<T> : IDisposable, IEnumerable<T> where T : IData, new()
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
        public ExtendibleHashingFile(string filePath, string overfillingFilePath, string managerFilePath,
            int blockByteSize = 4096, FileMode fileMode = FileMode.OpenOrCreate)
        {
            _file = new FileStream(filePath, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite, blockByteSize, FileOptions.WriteThrough);
            _overfillFile = new FileStream(overfillingFilePath, fileMode, FileAccess.ReadWrite);
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
                        block.Index *= 2;
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
            // TODO Co ak bude chciet vkladat do nealokovaneho bloku?????
            int index = HashCodeToIndex(itemAddress.GetHashCode());
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

        private int HashCodeToIndex(int hashCode)
        {
            BitArray bits = HashCodeToBitArray(hashCode);
            BitArray firstNBits = bits.FirstNLeastSignificantBits(_fileBitDepth);
            BitArray reversed = firstNBits.ReverseBits();
            return reversed.ToInt();
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

        // rozbi to na niekolo metod ! TODO
        private void Split(DataBlock<T> block)
        {
            // Find first index of this
            int firstIndexOfAddress = block.Index;
            while (firstIndexOfAddress > 0 && _blockAddresses[firstIndexOfAddress - 1] == block.InFileAddress)
            {
                firstIndexOfAddress--;
            }

            // Increment all blocks which has the same address
            int increaseFrom = firstIndexOfAddress;
            int countOfBlocksHavingSameAddress = (int)Math.Pow(2, _fileBitDepth - block.BitDepth);
            int increaseToExcluding = increaseFrom + countOfBlocksHavingSameAddress;
            for (int i = increaseFrom; i < increaseToExcluding; i++)
            {
                _blockBitDepths[i]++;
            }

            int updateNewAddressFrom = increaseFrom + (increaseToExcluding - increaseFrom) / 2;
            int updateNewAddressToExcliding = increaseToExcluding;

            DataBlock<T> block1 = new DataBlock<T>(block.Index, block.InFileAddress, _blockBitDepths[block.Index], _blockByteSize);
            DataBlock<T> block2 = GetNewBlock(updateNewAddressFrom);

            // Update the second half of edited blocks with new block address
            for (int i = updateNewAddressFrom; i < updateNewAddressToExcliding; i++)
            {
                _blockAddresses[i] = block2.InFileAddress;
            }

            foreach (var item in block)
            {
                int ind = HashCodeToIndex(item.GetHashCode());
                int address = _blockAddresses[ind];

                if (address == block1.InFileAddress)
                {
                    block1.Add(item);
                }
                else if (address == block2.InFileAddress)
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
