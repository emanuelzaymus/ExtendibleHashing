using ExtendibleHashing.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ExtendibleHashing
{
    class DataBlockFile<T> : IDisposable where T : IData, new()
    {
        private BinaryFileHandler _binFile;
        private readonly int _blockByteSize;

        public List<int> BlockAddresses { get; private set; } = new List<int>() { 0, 0 }; // adresar
        private List<int> _blockBitDepths = new List<int>() { 0, 0 }; // hlbky blokov

        private readonly List<bool> _blockOccupacity = new List<bool>() { true, false }; // obsadenost blokov

        public int BitDepth { get; private set; } = 1; // hlbka suboru

        //public DataBlockFile(string filePath, FileMode fileMode, int blockByteSize)
        //{
        //    _binFile = new BinaryFileHandler(filePath, fileMode, blockByteSize);
        //    _blockByteSize = blockByteSize;
        //}

        public DataBlockFile(string filePath, FileMode fileMode, int blockByteSize, TextFileHandler managerFile)
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
        }

        //public DataBlockFile(string filePath, FileMode fileMode, int blockByteSize,
        //    List<int> blockAddresses, List<int> blockBitDepths, List<bool> fileBlockOccupacity, int fileBitDepth)
        //    : this(filePath, fileMode, blockByteSize)
        //{
        //    _blockAddresses = blockAddresses;
        //    _blockBitDepths = blockBitDepths;
        //    _fileBlockOccupacity = fileBlockOccupacity;
        //    _fileBitDepth = fileBitDepth;
        //}

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

        // TODO: write at onece
        private void Save(DataBlock<T> block1, DataBlock<T> block2)
        {
            Save(block1);
            Save(block2);
        }

        public DataBlock<T> GetNewBlock(int index)
        {
            int indexOfEmptyBlock = _blockOccupacity.IndexOf(false);
            _blockOccupacity[indexOfEmptyBlock] = true;
            // If there is no more empty blocks -> Add one empty to the end.
            if (indexOfEmptyBlock + 1 == _blockOccupacity.Count)
            {
                _blockOccupacity.Add(false);
            }
            int newAddress = indexOfEmptyBlock * _blockByteSize;
            BlockAddresses[index] = newAddress;

            return new DataBlock<T>(index, newAddress, _blockBitDepths[index], _blockByteSize);
        }

        public void DoubleTheFileSize()
        {
            BlockAddresses = BlockAddresses.DoubleValues();
            _blockBitDepths = _blockBitDepths.DoubleValues();
            BitDepth++;
        }

        // rozbi to na niekolo metod ! TODO
        public void Split(DataBlock<T> block)
        {
            // Find first index of this
            int firstIndexOfAddress = block.Index;
            while (firstIndexOfAddress > 0 && BlockAddresses[firstIndexOfAddress - 1] == block.InFileAddress)
            {
                firstIndexOfAddress--;
            }

            // Increment all blocks which has the same address
            int increaseFrom = firstIndexOfAddress;
            int countOfBlocksHavingSameAddress = (int)Math.Pow(2, BitDepth - block.BitDepth);
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
                BlockAddresses[i] = block2.InFileAddress;
            }

            foreach (var item in block)
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
            Save(block1, block2);
            //Save(block1); // TODO: write at onece
            //Save(block2);
        }

        public void SaveManagerData(TextFileHandler managerFile)
        {
            managerFile.Write(_blockByteSize, BlockAddresses, _blockBitDepths, _blockOccupacity, BitDepth);
        }

        public void Dispose()
        {
            _binFile.Dispose();
        }

        private byte[] ReadBlock(int address)
        {
            return _binFile.ReadBlock(address);
        }

        // TODO Duplicity !!! - class Hashing
        private int HashCodeToIndex(int hashCode)
        {
            BitArray bits = HashCodeToBitArray(hashCode);
            BitArray firstNBits = bits.FirstNLeastSignificantBits(BitDepth);
            BitArray reversed = firstNBits.ReverseBits();
            return reversed.ToInt();
        }

        private BitArray HashCodeToBitArray(int hashCode)
        {
            return new BitArray(BitConverter.GetBytes(hashCode));
        }

    }
}
