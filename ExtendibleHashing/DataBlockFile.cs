using ExtendibleHashing.DataInterfaces;
using ExtendibleHashing.Extensions;
using ExtendibleHashing.FileHandlers;
using ExtendibleHashing.Hashing;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExtendibleHashing
{
    /// <summary>
    /// Main binary file that consists of DataBlocks.
    /// </summary>
    class DataBlockFile<T> : IDisposable where T : IData, new()
    {
        private readonly IHashing _hashing;
        private readonly BinaryFileHandler _binFile;
        public readonly int BlockByteSize;

        public List<int> BlockAddresses { get; private set; } = new List<int>() { 0, 0 }; // adresar
        public List<int> BlockBitDepths { get; private set; } = new List<int>() { 0, 0 }; // hlbky blokov
        private List<int> _blockItemCounts = new List<int>() { 0, 0 }; // obsadenie blokov poctom prvkov v blokoch

        public List<bool> BlockOccupation { get; } = new List<bool>() { true, false }; // obsadenost blokov

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
        public int MaxBitDepth { get; }

        public DataBlockFile(string filePath, FileMode fileMode, int blockByteSize, ManagingFileHandler managerFile, IHashing hashing, int maxBitDepth)
        {
            BlockByteSize = blockByteSize;
            MaxBitDepth = maxBitDepth;

            if (fileMode != FileMode.Create &&
                fileMode != FileMode.CreateNew &&
                managerFile.Read(out int bByteSize, out var bAddresses, out var bBitDepths, out var bItemCounts, out var bOccupation, out int bitDepth, out int mBitDepth))
            {
                BlockByteSize = bByteSize;
                BlockAddresses = bAddresses;
                BlockBitDepths = bBitDepths;
                _blockItemCounts = bItemCounts;
                BlockOccupation = bOccupation;
                BitDepth = bitDepth;
                MaxBitDepth = mBitDepth;
            }

            _binFile = new BinaryFileHandler(filePath, fileMode, BlockByteSize);
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
            return new DataBlock<T>(index, address, BlockBitDepths[index], data);
        }

        /// <summary>
        /// Saves <paramref name="block"/>.
        /// </summary>
        /// <param name="block"></param>
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

        /// <summary>
        /// Gets new block at index <paramref name="atIndex"/>.
        /// </summary>
        /// <param name="atIndex"></param>
        /// <returns></returns>
        public DataBlock<T> GetNewBlock(int atIndex)
        {
            int indexOfEmptyBlock = BlockOccupation.IndexOf(false);
            BlockOccupation[indexOfEmptyBlock] = true;
            // If there is no more empty blocks -> Add one empty to the end.
            if (indexOfEmptyBlock + 1 == BlockOccupation.Count)
            {
                BlockOccupation.Add(false);
            }
            int newAddress = indexOfEmptyBlock * BlockByteSize;
            BlockAddresses[atIndex] = newAddress;

            return new DataBlock<T>(atIndex, newAddress, BlockBitDepths[atIndex], BlockByteSize);
        }

        /// <summary>
        /// Doubles addresses, does not afect the binary file.
        /// </summary>
        public void DoubleTheFileSize()
        {
            BlockAddresses = BlockAddresses.DoubleValues();
            BlockBitDepths = BlockBitDepths.DoubleValues();
            _blockItemCounts = _blockItemCounts.DoubleValues();
            BitDepth++;
        }

        /// <summary>
        /// Splits <paramref name="block"/> into two blocks and saves them.
        /// </summary>
        /// <param name="block"></param>
        public void Split(DataBlock<T> block)
        {
            // Find first index of this block addres
            int firstIndexOfAddress = GetFirstIndexOfBlockAddress(block);

            int countOfBlocksHavingSameAddress = GetCountOfBlocksHavingSameAddress(block);

            // Increment all blocks which have the same address
            IncrementBlockBitDepths(firstIndexOfAddress, countOfBlocksHavingSameAddress);

            int setNewAddressFromIndex = firstIndexOfAddress + countOfBlocksHavingSameAddress / 2;

            // I'm putting firstIndexOfAddress into the oldBlock constructor because block.Index can be from newBlock.Index range
            DataBlock<T> oldBlock = new DataBlock<T>(firstIndexOfAddress, block.InFileAddress, BlockBitDepths[firstIndexOfAddress], BlockByteSize); // Empty copy of original block
            DataBlock<T> newBlock = GetNewBlock(setNewAddressFromIndex); // Allocate new block

            // Update the second half of edited blocks with new block address
            SetBlockAdresses(setNewAddressFromIndex, countOfBlocksHavingSameAddress / 2, newBlock.InFileAddress);

            DivideBlockIntoTwoBlocks(block, oldBlock, newBlock);


            void IncrementBlockBitDepths(int indexFrom, int count)
            {
                for (int i = indexFrom; i < indexFrom + count; i++)
                    BlockBitDepths[i]++;
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

        /// <summary>
        /// Merges.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="addressesWithOverfillingBlocks"></param>
        /// <returns></returns>
        public bool TryMergeAndSave(DataBlock<T> block, List<int> addressesWithOverfillingBlocks)
        {
            bool saved = false;
            while (true)
            {
                if (IsPossibleToMergeWithNeighbourBlock(block, addressesWithOverfillingBlocks, out int neighbourIndex))
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
            if (!BlockBitDepths.Contains(BitDepth))
            {
                BlockAddresses = BlockAddresses.RemoveEveryOtherValue();
                BlockBitDepths = BlockBitDepths.RemoveEveryOtherValue();
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
            _binFile.ReduceSizeIfPossible((BlockOccupation.LastIndexOf(true) + 1) * BlockByteSize);
        }

        private void DecrementBitDepths(DataBlock<T> block)
        {
            int first = GetFirstIndexOfBlockAddress(block);
            int lastExclude = first + GetCountOfBlocksHavingSameAddress(block);
            for (int i = first; i < lastExclude; i++)
            {
                BlockBitDepths[i]--;
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
            int blockOccupationIndex = address / BlockByteSize;
            BlockOccupation[blockOccupationIndex] = false;

            // If last two blocks are empty remove the last one -> at the end keep one empty for future new DataBlock
            while (BlockOccupation[BlockOccupation.Count - 1] == false
                && BlockOccupation[BlockOccupation.Count - 2] == false)
            {
                BlockOccupation.RemoveAt(BlockOccupation.Count - 1);
            }
        }

        private bool IsPossibleToMergeWithNeighbourBlock(DataBlock<T> block, List<int> addressesWithOverfillingBlocks, out int foundNeighbourIndex)
        {
            int? neighbourIndex = GetNeighbourIndex(block);
            // If address of found index is in overfillinf file it cannot be merged.
            if (neighbourIndex.HasValue && !addressesWithOverfillingBlocks.Contains(BlockAddresses[neighbourIndex.Value]))
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
                    block.BitDepth == BlockBitDepths[leftNeighbourIndex] &&
                    SameBitOnNthPosition(block.Index, leftNeighbourIndex, block.BitDepth))
                {
                    return leftNeighbourIndex;
                }
                else if (rightNeighbourIndex < BlockBitDepths.Count
                    && block.BitDepth == BlockBitDepths[rightNeighbourIndex]
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

        public void SaveManagingData(ManagingFileHandler managingFile)
        {
            managingFile.Write(BlockByteSize, BlockAddresses, BlockBitDepths, _blockItemCounts, BlockOccupation, BitDepth, MaxBitDepth);
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
