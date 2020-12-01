using ExtendibleHashing.DataInterfaces;
using ExtendibleHashing.Extensions;
using ExtendibleHashing.FileHandlers;
using ExtendibleHashing.Hashing;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExtendibleHashing
{
    class DataBlockFile<T> : IDisposable where T : IData, new()
    {
        private readonly IHashing _hashing;
        private readonly BinaryFileHandler _binFile;
        private readonly int _blockByteSize;

        public List<int> BlockAddresses { get; private set; } = new List<int>() { 0, 0 }; // adresar
        private List<int> _blockBitDepths = new List<int>() { 0, 0 }; // hlbky blokov

        private readonly List<bool> _blockOccupacity = new List<bool>() { true, false }; // obsadenost blokov

        public int BitDepth { get; private set; } = 1; // hlbka suboru

        public DataBlockFile(string filePath, FileMode fileMode, int blockByteSize, TextFileHandler managerFile, IHashing hashing)
        {
            _blockByteSize = blockByteSize;

            if (fileMode != FileMode.Create &&
                fileMode != FileMode.CreateNew &&
                managerFile.Read(out int bByteSize, out var bAddresses, out var bBitDepths, out var bOccupacity, out int bitDepth))
            {
                _blockByteSize = bByteSize;
                BlockAddresses = bAddresses;
                _blockBitDepths = bBitDepths;
                _blockOccupacity = bOccupacity;
                BitDepth = bitDepth;
            }

            _binFile = new BinaryFileHandler(filePath, fileMode, _blockByteSize);
            _hashing = hashing;
        }

        public DataBlock<T> GetDataBlock(T itemId)
        {
            int index = HashCodeToIndex(itemId.GetHashCode());
            int address = BlockAddresses[index];

            byte[] data = ReadBlock(address);
            return new DataBlock<T>(index, address, _blockBitDepths[index], data);
        }

        public DataBlock<T> GetDataBlock(int address)
        {
            byte[] data = ReadBlock(address);
            return new DataBlock<T>(data);
        }

        public void Save(DataBlock<T> block)
        {
            _binFile.Save(block.ToByteArray(), block.InFileAddress);
        }

        public DataBlock<T> GetNewBlock(int atIndex)
        {
            int indexOfEmptyBlock = _blockOccupacity.IndexOf(false);
            _blockOccupacity[indexOfEmptyBlock] = true;
            // If there is no more empty blocks -> Add one empty to the end.
            if (indexOfEmptyBlock + 1 == _blockOccupacity.Count)
            {
                _blockOccupacity.Add(false);
            }
            int newAddress = indexOfEmptyBlock * _blockByteSize;
            BlockAddresses[atIndex] = newAddress;

            return new DataBlock<T>(atIndex, newAddress, _blockBitDepths[atIndex], _blockByteSize);
        }

        public void DoubleTheFileSize()
        {
            BlockAddresses = BlockAddresses.DoubleValues();
            _blockBitDepths = _blockBitDepths.DoubleValues();
            BitDepth++;
        }

        public void Split(DataBlock<T> block)
        {
            // Find first index of this block addres
            int firstIndexOfAddress = GetFirstIndexOfBlockAddress(block);

            int countOfBlocksHavingSameAddress = (int)Math.Pow(2, BitDepth - block.BitDepth);

            // Increment all blocks which have the same address
            IncrementBlockBitDepths(firstIndexOfAddress, countOfBlocksHavingSameAddress);

            int setNewAddressFromIndex = firstIndexOfAddress + countOfBlocksHavingSameAddress / 2;

            DataBlock<T> oldBlock = new DataBlock<T>(block.Index, block.InFileAddress, _blockBitDepths[block.Index], _blockByteSize); // Empty copy of original block
            DataBlock<T> newBlock = GetNewBlock(setNewAddressFromIndex); // Allocate new block

            // Update the second half of edited blocks with new block address
            SetBlockAdresses(setNewAddressFromIndex, countOfBlocksHavingSameAddress / 2, newBlock.InFileAddress);

            DivideBlockIntoTwoBlocks(block, oldBlock, newBlock);


            int GetFirstIndexOfBlockAddress(DataBlock<T> b)
            {
                int frstIndexOfAddress = b.Index;
                while (frstIndexOfAddress > 0 && BlockAddresses[frstIndexOfAddress - 1] == b.InFileAddress)
                {
                    frstIndexOfAddress--;
                }
                return frstIndexOfAddress;
            }

            void IncrementBlockBitDepths(int indexFrom, int count)
            {
                for (int i = indexFrom; i < indexFrom + count; i++)
                    _blockBitDepths[i]++;
            }

            void SetBlockAdresses(int indexFrom, int count, int newAddress)
            {
                for (int i = indexFrom; i < indexFrom + count; i++)
                    BlockAddresses[i] = newAddress;
            }

            void DivideBlockIntoTwoBlocks(DataBlock<T> blockToDivide, DataBlock<T> block1, DataBlock<T> block2)
            {
                foreach (var item in blockToDivide)
                {
                    int ind = HashCodeToIndex(item.GetHashCode());
                    int address = BlockAddresses[ind];

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
                Save(block1);
                Save(block2);
            }
        }

        public void TryMerge(DataBlock<T> block)
        {
            throw new NotImplementedException(); // TODO continue with deletion 
        }

        public void SaveManagingData(TextFileHandler managingFile)
        {
            managingFile.Write(_blockByteSize, BlockAddresses, _blockBitDepths, _blockOccupacity, BitDepth);
        }

        public void Dispose()
        {
            _binFile.Dispose();
        }

        private byte[] ReadBlock(int address)
        {
            return _binFile.ReadBlock(address);
        }

        private int HashCodeToIndex(int hashCode)
        {
            return _hashing.HashCodeToIndex(hashCode, BitDepth);
        }

    }
}
