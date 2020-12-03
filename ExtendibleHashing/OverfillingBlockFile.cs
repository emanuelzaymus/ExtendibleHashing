using ExtendibleHashing.DataInterfaces;
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

        private readonly List<bool> _blockOccupation = new List<bool>() { false }; // Occupaci of blocks.

        public OverfillingBlockFile(string overfillingFilePath, FileMode fileMode, int blockByteSize, OverfillingManagingFileHandler managingFile)
        {
            _blockByteSize = blockByteSize;

            //if (fileMode != FileMode.Create &&
            //    fileMode != FileMode.CreateNew &&
            //    managingFile.Read(out int bByteSize))
            //{
            //    _blockByteSize = bByteSize;
            //    BlockAddresses = bAddresses;
            //    _blockBitDepths = bBitDepths;
            //    _blockItemCounts = bItemCounts;
            //    _blockOccupation = bOccupation;
            //}

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
            //throw new NotImplementedException();
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
