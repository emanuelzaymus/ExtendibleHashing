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
        private List<int> _blockItemCounts = new List<int>() { 0, 0 }; // obsadenie blokov poctom prvkov v blokoch

        private readonly List<bool> _blockOccupation = new List<bool>() { true, false }; // obsadenost blokov

        private int _bitDepth = 1;
        public int BitDepth  // hlbka suboru
        {
            get => _bitDepth;
            private set
            {
                if (value >= 1)
                    _bitDepth = value;
                else throw new ArgumentException($"{nameof(BitDepth)} can not be less than 1.");
            }
        }

        public DataBlockFile(string filePath, FileMode fileMode, int blockByteSize, TextFileHandler managerFile, IHashing hashing)
        {
            _blockByteSize = blockByteSize;

            if (fileMode != FileMode.Create &&
                fileMode != FileMode.CreateNew &&
                managerFile.Read(out int bByteSize, out var bAddresses, out var bBitDepths, out var bItemCounts, out var bOccupation, out int bitDepth))
            {
                _blockByteSize = bByteSize;
                BlockAddresses = bAddresses;
                _blockBitDepths = bBitDepths;
                _blockItemCounts = bItemCounts;
                _blockOccupation = bOccupation;
                BitDepth = bitDepth;
            }

            _binFile = new BinaryFileHandler(filePath, fileMode, _blockByteSize);
            _hashing = hashing;
        }

        public DataBlock<T> GetDataBlock(T itemId)
        {
            int index = HashCodeToIndex(itemId.GetHashCode());
            return GetDataBlockByIndex(index);
        }

        /// <summary>
        /// Gets DataBlock on <paramref name="address"/> with invalid properties.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public DataBlock<T> GetDataBlock(int address)
        {
            byte[] data = ReadBlock(address);
            return new DataBlock<T>(data);
        }

        private DataBlock<T> GetDataBlockByIndex(int index)
        {
            int address = BlockAddresses[index];
            byte[] data = ReadBlock(address);
            return new DataBlock<T>(index, address, _blockBitDepths[index], data);
        }

        public void Save(DataBlock<T> block)
        {
            UpdateBlockItemCounts(block);
            _binFile.Save(block.ToByteArray(), block.InFileAddress);
        }

        private void UpdateBlockItemCounts(DataBlock<T> block)
        {
            int first = GetFirstIndexOfBlockAddress(block);
            int lastExclude = first + GetCountOfBlocksHavingSameAddress(block);
            for (int i = first; i < lastExclude; i++)
            {
                _blockItemCounts[i] = block.ItemCount;
            }
        }

        public DataBlock<T> GetNewBlock(int atIndex)
        {
            int indexOfEmptyBlock = _blockOccupation.IndexOf(false);
            _blockOccupation[indexOfEmptyBlock] = true;
            // If there is no more empty blocks -> Add one empty to the end.
            if (indexOfEmptyBlock + 1 == _blockOccupation.Count)
            {
                _blockOccupation.Add(false);
            }
            int newAddress = indexOfEmptyBlock * _blockByteSize;
            BlockAddresses[atIndex] = newAddress;

            return new DataBlock<T>(atIndex, newAddress, _blockBitDepths[atIndex], _blockByteSize);
        }

        public void DoubleTheFileSize()
        {
            BlockAddresses = BlockAddresses.DoubleValues();
            _blockBitDepths = _blockBitDepths.DoubleValues();
            _blockItemCounts = _blockItemCounts.DoubleValues();
            BitDepth++;
        }

        public void Split(DataBlock<T> block)
        {
            // Find first index of this block addres
            int firstIndexOfAddress = GetFirstIndexOfBlockAddress(block);

            int countOfBlocksHavingSameAddress = GetCountOfBlocksHavingSameAddress(block);

            // Increment all blocks which have the same address
            IncrementBlockBitDepths(firstIndexOfAddress, countOfBlocksHavingSameAddress);

            int setNewAddressFromIndex = firstIndexOfAddress + countOfBlocksHavingSameAddress / 2;

            // I'm putting firstIndexOfAddress into the oldBlock constructor because block.Index can be from newBlock.Index range
            DataBlock<T> oldBlock = new DataBlock<T>(firstIndexOfAddress, block.InFileAddress, _blockBitDepths[firstIndexOfAddress], _blockByteSize); // Empty copy of original block
            DataBlock<T> newBlock = GetNewBlock(setNewAddressFromIndex); // Allocate new block

            // Update the second half of edited blocks with new block address
            SetBlockAdresses(setNewAddressFromIndex, countOfBlocksHavingSameAddress / 2, newBlock.InFileAddress);

            DivideBlockIntoTwoBlocks(block, oldBlock, newBlock);


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
                    else throw new Exception("You should not get here.");
                }
                Save(block1);
                Save(block2);
            }
        }

        private int GetFirstIndexOfBlockAddress(DataBlock<T> b)
        {
            int frstIndexOfAddress = b.Index;
            // While current index is not 0 and next address is still the same, move to the next index.
            while (frstIndexOfAddress > 0 && BlockAddresses[frstIndexOfAddress - 1] == b.InFileAddress)
            {
                frstIndexOfAddress--;
            }
            return frstIndexOfAddress;
        }

        private int GetCountOfBlocksHavingSameAddress(DataBlock<T> block)
        {
            return (int)Math.Pow(2, BitDepth - block.BitDepth);
        }

        public bool TryMergeAndSave(DataBlock<T> block)
        {
            bool saved = false;
            while (true)
            {
                if (IsPossibleToMergeWithNeighbourBlock(block, out int neighbourIndex))
                {
                    var neighbourBlock = GetDataBlockByIndex(neighbourIndex);
                    block = MergeBlocks(block, neighbourBlock);
                    Save(block);
                    saved = true;
                    if (TryShrinkFile())
                    {
                        block.Index /= 2;
                    }
                }
                else break;
            }
            return saved;
        }

        private bool TryShrinkFile()
        {
            if (!_blockBitDepths.Contains(BitDepth))
            {
                BlockAddresses = BlockAddresses.RemoveEveryOtherValue();
                _blockBitDepths = _blockBitDepths.RemoveEveryOtherValue();
                _blockItemCounts = _blockItemCounts.RemoveEveryOtherValue();
                BitDepth--;
                return true;
            }
            return false;
        }

        private DataBlock<T> MergeBlocks(DataBlock<T> block, DataBlock<T> neighbourBlock)
        {
            DataBlock<T> blockWithLowerAddress;
            DataBlock<T> blockWithHigherAddress;
            if (block.InFileAddress < neighbourBlock.InFileAddress)
            {
                blockWithLowerAddress = block;
                blockWithHigherAddress = neighbourBlock;
            }
            else if (block.InFileAddress > neighbourBlock.InFileAddress)
            {
                blockWithLowerAddress = neighbourBlock;
                blockWithHigherAddress = block;
            }
            else throw new Exception("You should not get here.");

            foreach (var item in blockWithHigherAddress)
            {
                blockWithLowerAddress.Add(item);
            }
            // Set unused address free.
            FreeUpBlockAddress(blockWithHigherAddress.InFileAddress);
            ReduceFileSizeIfPossible();
            // Replace all blockWithHigherAddress items to blockWithLowerAddress properties.
            SetBlockAddresses(blockWithHigherAddress, blockWithLowerAddress.InFileAddress);
            blockWithHigherAddress.InFileAddress = blockWithLowerAddress.InFileAddress;
            // Decrement all bit depths where is new address.
            blockWithLowerAddress.BitDepth--;
            blockWithHigherAddress.BitDepth--;
            DecrementBitDepths(blockWithHigherAddress);
            // Update block item counts for 
            UpdateBlockItemCounts(blockWithLowerAddress);

            return blockWithLowerAddress;
        }

        private void ReduceFileSizeIfPossible()
        {
            _binFile.ReduceSizeIfPossible((_blockOccupation.LastIndexOf(true) + 1) * _blockByteSize);
        }

        private void DecrementBitDepths(DataBlock<T> block)
        {
            int first = GetFirstIndexOfBlockAddress(block);
            int lastExclude = first + GetCountOfBlocksHavingSameAddress(block);
            for (int i = first; i < lastExclude; i++)
            {
                _blockBitDepths[i]--;
            }
        }

        private void SetBlockAddresses(DataBlock<T> oldBlock, int newAddress)
        {
            int first = GetFirstIndexOfBlockAddress(oldBlock);
            int lastExclude = first + GetCountOfBlocksHavingSameAddress(oldBlock);
            for (int i = first; i < lastExclude; i++)
            {
                BlockAddresses[i] = newAddress;
            }
        }

        private void FreeUpBlockAddress(int address)
        {
            int blockOccupationIndex = address / _blockByteSize;
            _blockOccupation[blockOccupationIndex] = false;

            // If last two blocks are empty remove the last one -> at the end keep one empty for future new DataBlock
            while (_blockOccupation[_blockOccupation.Count - 1] == false
                && _blockOccupation[_blockOccupation.Count - 2] == false)
            {
                _blockOccupation.RemoveAt(_blockOccupation.Count - 1);
            }
        }

        private bool IsPossibleToMergeWithNeighbourBlock(DataBlock<T> block, out int foundNeighbourIndex)
        {
            int? neighbourIndex = GetNeighbourIndex(block);
            if (neighbourIndex.HasValue)
            {
                int maxCount = block.MaxItemCount;
                foundNeighbourIndex = neighbourIndex.Value;
                return block.ItemCount + _blockItemCounts[neighbourIndex.Value] <= maxCount;
            }
            foundNeighbourIndex = -1;
            return false;
        }

        private int? GetNeighbourIndex(DataBlock<T> block)
        {
            if (block.BitDepth > 1)
            {
                int firstIndexOfBlockAddress = GetFirstIndexOfBlockAddress(block);
                int countOfBlockcHavingSameAddress = GetCountOfBlocksHavingSameAddress(block);

                int leftNeighbourIndex = firstIndexOfBlockAddress - 1;
                int rightNeighbourIndex = firstIndexOfBlockAddress + countOfBlockcHavingSameAddress;

                if (leftNeighbourIndex > 0 &&
                    block.BitDepth == _blockBitDepths[leftNeighbourIndex] &&
                    SameBitOnNthPosition(block.Index, leftNeighbourIndex, block.BitDepth))
                {
                    return leftNeighbourIndex;
                }
                else if (rightNeighbourIndex < _blockBitDepths.Count
                    && block.BitDepth == _blockBitDepths[rightNeighbourIndex]
                    && SameBitOnNthPosition(block.Index, rightNeighbourIndex, block.BitDepth))
                {
                    return rightNeighbourIndex;
                }
            }
            return null;
        }

        private bool SameBitOnNthPosition(int index1, int index2, int n)
        {
            int mask = (int)Math.Pow(2, n);
            return (index1 & mask) == (index2 & mask);
        }

        public void SaveManagingData(TextFileHandler managingFile)
        {
            managingFile.Write(_blockByteSize, BlockAddresses, _blockBitDepths, _blockItemCounts, _blockOccupation, BitDepth);
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
