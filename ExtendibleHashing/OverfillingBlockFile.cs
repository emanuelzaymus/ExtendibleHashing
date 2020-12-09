using ExtendibleHashing.DataInterfaces;
using ExtendibleHashing.Extensions;
using ExtendibleHashing.FileHandlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExtendibleHashing
{
    class OverfillingBlockFile<T> : IDisposable, IEnumerable<T> where T : IData, new()
    {
        private readonly int _blockByteSize;

        private readonly BinaryFileHandler _file;

        private readonly List<List<OverfillingBlockInfo>> _blocksInfo = new List<List<OverfillingBlockInfo>>(); // Information about overfilling blocks.

        private readonly List<bool> _blockOccupation = new List<bool>() { false }; // Occupation of blocks.

        public int? BlockMaxItemCount
        {
            get
            {
                foreach (var infoList in _blocksInfo)
                {
                    foreach (var info in infoList)
                        return info.MaxItemCount;
                }
                return null;
            }
        }

        public OverfillingBlockFile(string overfillingFilePath, FileMode fileMode, int blockByteSize, OverfillingManagingFileHandler managingFile)
        {
            _blockByteSize = blockByteSize;

            if (fileMode != FileMode.Create &&
                fileMode != FileMode.CreateNew &&
                managingFile.Read(out int bByteSize, out List<bool> fBlockOccupation, out List<List<OverfillingBlockInfo>> bInfo))
            {
                _blockByteSize = bByteSize;
                _blockOccupation = fBlockOccupation;
                _blocksInfo = bInfo;
            }

            _file = new BinaryFileHandler(overfillingFilePath, fileMode, _blockByteSize);
        }

        internal void Add(int mainFileAddress, T item)
        {
            var blockInfo = GetNotFullBlockInfo(mainFileAddress);
            OverfillingBlock<T> block;
            if (blockInfo != null)
            {
                block = !blockInfo.IsFull
                    ? LoadBlock(blockInfo)
                    : ExtendBlockSeries(blockInfo.MainFileAddress);
            }
            else
            {
                block = CreateNewBlockSeries(mainFileAddress);
            }
            block.Add(item);
            Save(block);
        }

        private void Save(OverfillingBlock<T> block)
        {
            _file.Save(block.ToByteArray(), block.Address);
        }

        private OverfillingBlock<T> ExtendBlockSeries(int mainFileAddress)
        {
            int newAddress = GetNewAddress();
            var newBlockInfo = new OverfillingBlockInfo(mainFileAddress, newAddress);
            GetBlocksInfoSeries(mainFileAddress).Add(newBlockInfo);
            return new OverfillingBlock<T>(_blockByteSize, newBlockInfo);
        }

        private List<OverfillingBlockInfo> GetBlocksInfoSeries(int mainFileAddress)
        {
            foreach (var infoSeries in _blocksInfo)
            {
                if (infoSeries[0].MainFileAddress == mainFileAddress)
                    return infoSeries;
            }
            return null;
        }

        private OverfillingBlock<T> LoadBlock(OverfillingBlockInfo blockInfo)
        {
            byte[] data = _file.ReadBlock(blockInfo.Address);
            return new OverfillingBlock<T>(_blockByteSize, blockInfo, data);
        }

        internal bool ContainsAddress(int mainFileAddress)
        {
            return GetBlocksInfoSeries(mainFileAddress) != null;
        }

        internal List<int> GetAllMainFileAddresses()
        {
            var ret = new List<int>();
            foreach (var infoList in _blocksInfo)
            {
                ret.Add(infoList[0].MainFileAddress);
            }
            return ret;
        }

        internal T Find(int mainFileAddress, T itemId)
        {
            List<OverfillingBlockInfo> infoList = GetBlocksInfoSeries(mainFileAddress);
            if (infoList != null)
            {
                foreach (var blockInfo in infoList)
                {
                    OverfillingBlock<T> block = LoadBlock(blockInfo);
                    T found = block.Find(itemId);
                    if (found != null)
                        return found;
                }
            }
            return default;
        }

        internal bool Remove(int mainFileAddress, T itemId)
        {
            List<OverfillingBlockInfo> infoList = GetBlocksInfoSeries(mainFileAddress);
            if (infoList != null)
            {
                foreach (var info in infoList)
                {
                    var block = LoadBlock(info);
                    if (block.Remove(itemId))
                    {
                        Save(block);
                        return true;
                    }
                }
            }
            return false;
        }

        internal List<T> ShrinkAndGetItems(int mainFileAddress, int countToReturn)
        {
            List<OverfillingBlockInfo> infoList = GetBlocksInfoSeries(mainFileAddress);
            if (infoList == null)
                throw new Exception("Address does not exist.");

            List<T> collectedItems = GetAllItems(infoList);
            infoList.ForEach(info => info.ItemCount = 0);

            if (collectedItems.Count <= countToReturn)
            {
                Shrink(mainFileAddress);
                ReduceFileSizeIfPossible();
                return collectedItems;
            }

            // Fill ret list
            List<T> ret = new List<T>(countToReturn);
            while (ret.Count < countToReturn)
            {
                ret.Add(collectedItems.PopLast());
            }

            foreach (var info in infoList.OrderBy(i => i.Address))
            {
                OverfillingBlock<T> block = new OverfillingBlock<T>(_blockByteSize, info);
                while (!block.IsFull && collectedItems.Count > 0)
                    block.Add(collectedItems.PopFirst());

                Save(block);

                if (collectedItems.Count == 0)
                    break;
            }
            Shrink(mainFileAddress);
            ReduceFileSizeIfPossible();
            return ret;
        }

        internal bool Update(int mainFileAddress, T newItem)
        {
            List<OverfillingBlockInfo> infoList = GetBlocksInfoSeries(mainFileAddress);
            if (infoList != null)
            {
                foreach (var blockInfo in infoList)
                {
                    OverfillingBlock<T> block = LoadBlock(blockInfo);
                    if (block.Update(newItem))
                    {
                        Save(block);
                        return true;
                    }
                }
            }
            return false;
        }

        private void ReduceFileSizeIfPossible()
        {
            _file.ReduceSizeIfPossible((_blockOccupation.LastIndexOf(true) + 1) * _blockByteSize);
        }


        private void Shrink(int mainFileAddress)
        {
            for (int i = 0; i < _blocksInfo.Count; i++)
            {
                List<OverfillingBlockInfo> infoSeries = _blocksInfo[i];
                if (infoSeries[0].MainFileAddress == mainFileAddress)
                {
                    for (int j = 0; j < infoSeries.Count; j++)
                    {
                        if (infoSeries[j].ItemCount == 0)
                        {
                            FreeUpBlockAddress(infoSeries[j].Address);
                            infoSeries.RemoveAt(j--);
                        }
                    }
                }
                if (infoSeries.Count == 0)
                    _blocksInfo.RemoveAt(i--);
            }
        }

        private void FreeUpBlockAddress(int address)
        {
            int blockOccupationIndex = address / _blockByteSize;
            _blockOccupation[blockOccupationIndex] = false;

            // If last two blocks are empty remove the last one -> at the end keep one empty for future new DataBlock
            while (_blockOccupation.Count > 2
                && _blockOccupation[_blockOccupation.Count - 1] == false
                && _blockOccupation[_blockOccupation.Count - 2] == false)
            {
                _blockOccupation.RemoveAt(_blockOccupation.Count - 1);
            }
        }

        private List<T> GetAllItems(List<OverfillingBlockInfo> infoList)
        {
            List<T> collectedItems = new List<T>();
            if (infoList != null)
            {
                foreach (var info in infoList)
                {
                    if (info.ItemCount != 0)
                    {
                        OverfillingBlock<T> overfillingBlocks = LoadBlock(info);
                        collectedItems.AddRange(overfillingBlocks);
                    }
                }
            }
            return collectedItems;
        }

        internal int BlockCountForAddress(int mainFileAddress)
        {
            List<OverfillingBlockInfo> infoList = GetBlocksInfoSeries(mainFileAddress);
            if (infoList != null)
            {
                return infoList.Count;
            }
            return 0;
        }

        internal int ItemCountForAddress(int mainFileAddress)
        {
            List<OverfillingBlockInfo> infoList = GetBlocksInfoSeries(mainFileAddress);
            if (infoList != null)
            {
                return infoList.Select(il => il.ItemCount).Sum();
            }
            return 0;
        }

        private OverfillingBlockInfo GetNotFullBlockInfo(int mainFileAddress)
        {
            var infoList = GetBlocksInfoSeries(mainFileAddress);
            if (infoList != null)
            {
                var foundBlockInfo = infoList.FirstOrDefault(bi => !bi.IsFull);
                if (foundBlockInfo != null)
                {
                    return foundBlockInfo;
                }
                return infoList.Last();
            }
            return null;
        }

        private OverfillingBlock<T> CreateNewBlockSeries(int mainFileAddress)
        {
            int newAddress = GetNewAddress();
            OverfillingBlockInfo info = new OverfillingBlockInfo(mainFileAddress, newAddress);
            _blocksInfo.Add(new List<OverfillingBlockInfo>() { info });
            return new OverfillingBlock<T>(_blockByteSize, info);
        }

        private int GetNewAddress()
        {
            int emptyBlockIndex = _blockOccupation.IndexOf(false);
            _blockOccupation[emptyBlockIndex] = true;
            // If there is no more empty blocks -> Add one empty to the end.
            if (emptyBlockIndex + 1 == _blockOccupation.Count)
            {
                _blockOccupation.Add(false);
            }
            return emptyBlockIndex * _blockByteSize;
        }

        internal void SaveManagingData(OverfillingManagingFileHandler overfillingManagingFile)
        {
            overfillingManagingFile.Write(_blockByteSize, _blockOccupation, _blocksInfo);
        }

        public void Dispose()
        {
            _file.Dispose();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var infoList in _blocksInfo)
            {
                foreach (var info in infoList)
                {
                    foreach (var item in LoadBlock(info))
                        yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
