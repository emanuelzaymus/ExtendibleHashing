using ExtendibleHashing.DataInterfaces;
using ExtendibleHashing.FileHandlers;
using ExtendibleHashing.Hashing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ExtendibleHashing
{
    public class ExtendibleHashingFile<T> : IDisposable, IEnumerable<T> where T : IData, new()
    {
        private readonly ManagingFileHandler _managingFile;
        private readonly OverfillingManagingFileHandler _overfillingManagingFile;
        private readonly DataBlockFile<T> _file;
        private readonly OverfillingBlockFile<T> _overfillFile;

        /// <summary>
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="overfillingFilePath"></param>
        /// <param name="managerFilePath"></param>
        /// <param name="blockByteSize">Block size in bytes</param>
        public ExtendibleHashingFile(string filePath, string overfillingFilePath, string managerFilePath, string overfillingManagerFilePath,
            int blockByteSize = 4096, int overfillingBlockByteSize = 4096, FileMode fileMode = FileMode.OpenOrCreate, int maxBitDepth = 32)
        {
            _managingFile = new ManagingFileHandler(managerFilePath);
            _overfillingManagingFile = new OverfillingManagingFileHandler(overfillingManagerFilePath);
            _file = new DataBlockFile<T>(filePath, fileMode, blockByteSize, _managingFile, new BitHashing(), maxBitDepth);
            _overfillFile = new OverfillingBlockFile<T>(overfillingFilePath, fileMode, overfillingBlockByteSize, _overfillingManagingFile);
        }

        /// <summary>
        /// Adds <paramref name="item"/> into the file if such item doe not exist.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            while (true)
            {
                var block = _file.GetDataBlock(item);

                if (block.IsFull)
                {
                    if (_file.BitDepth == block.BitDepth)
                    {
                        if (_file.BitDepth < _file.MaxBitDepth)
                        {
                            _file.DoubleTheFileSize();
                            block.Index *= 2;
                            _file.Split(block);
                        }
                        else
                        {
                            _overfillFile.Add(block.InFileAddress, item);
                            break;
                        }
                    }
                    else
                    {
                        _file.Split(block);
                    }
                }
                else
                {
                    if (!_overfillFile.ContainsItem(block.InFileAddress, item))
                    {
                        block.Add(item);
                        _file.Save(block);
                        break;
                    }
                    else throw new ArgumentException("This item is already present in the file.");
                }
            }
        }

        /// <summary>
        /// Finds item based on id of <paramref name="itemId"/> or null.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public T Find(T itemId)
        {
            var block = _file.GetDataBlock(itemId);
            T foundItem = block.Find(itemId);
            if (foundItem == null)
            {
                foundItem = _overfillFile.Find(block.InFileAddress, itemId);
            }
            return foundItem;
        }

        /// <summary>
        /// Removes item with id of <paramref name="itemId"/> and returns success.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool Remove(T itemId)
        {
            bool wasRemoved;
            var block = _file.GetDataBlock(itemId);
            if (block.Remove(itemId))
            {
                bool mergedAndSaved = false;
                if (!_overfillFile.ContainsAddress(block.InFileAddress))
                {
                    mergedAndSaved = _file.TryMergeAndSave(block, _overfillFile.GetAllMainFileAddresses());
                }
                if (!mergedAndSaved)
                {
                    _file.Save(block);
                }
                wasRemoved = true;
            }
            else wasRemoved = _overfillFile.Remove(block.InFileAddress, itemId);

            if (wasRemoved)
            {
                int overfillBlockCount = _overfillFile.BlockCountForAddress(block.InFileAddress);
                if (overfillBlockCount > 0)
                {
                    int freeItemsInMainBlock = block.MaxItemCount - block.ItemCount;

                    int itemCountForAddress = _overfillFile.ItemCountForAddress(block.InFileAddress);
                    int blockMaxItemCount = _overfillFile.BlockMaxItemCount.Value;

                    // May I shrink overfillig file? Can shrink the file only if it saves at least one free block.
                    int freeItems = overfillBlockCount * blockMaxItemCount - itemCountForAddress;
                    if (freeItems + freeItemsInMainBlock >= blockMaxItemCount || block.ItemCount == 0)
                    {
                        var items = _overfillFile.ShrinkAndGetItems(block.InFileAddress, freeItemsInMainBlock);
                        if (items.Count > 0)
                        {
                            items.ForEach(i => block.Add(i));
                            _file.Save(block);
                        }
                    }
                }
            }
            return wasRemoved;
        }

        /// <summary>
        /// Updates <paramref name="oldItem"/> with <paramref name="newItem"/>. Both items have to have the same Ids.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        public bool Update(T oldItem, T newItem)
        {
            if (!oldItem.IdEquals(newItem))
            {
                throw new ArgumentException($"Parameters {nameof(oldItem)} and {nameof(newItem)} do not equal in ID attributes.");
            }

            var block = _file.GetDataBlock(oldItem);
            if (block.Update(newItem))
            {
                _file.Save(block);
                return true;
            }
            return _overfillFile.Update(block.InFileAddress, newItem);
        }

        /// <summary>
        /// Saves all managing dat and disposses all files.
        /// </summary>
        public void Dispose()
        {
            _file.SaveManagingData(_managingFile);
            _file.Dispose();
            _overfillFile.SaveManagingData(_overfillingManagingFile);
            _overfillFile.Dispose();
        }

        public IEnumerator<T> GetEnumerator()
        {
            int lastAddr = -1;
            foreach (var addr in _file.BlockAddresses)
            {
                if (addr != lastAddr)
                {
                    lastAddr = addr;

                    var block = _file.GetDataBlock(addr);
                    foreach (var item in block)
                        yield return item;
                }
            }
            foreach (var item in _overfillFile)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<PresentableBlockItem<T>> MainFileItems()
        {
            for (int i = 0; i < _file.BlockOccupation.Count - 1; i++)
            {
                int addr = i * _file.BlockByteSize;
                if (_file.BlockOccupation[i])
                {
                    var block = _file.GetDataBlock(addr);
                    foreach (var item in block)
                    {
                        yield return new PresentableBlockItem<T>(i, addr, _file.BlockBitDepths[i], item);
                    }
                    for (int j = block.ItemCount; j < block.MaxItemCount; j++)
                    {
                        yield return new PresentableBlockItem<T>(i, addr, _file.BlockBitDepths[i], default);
                    }
                }
                else
                {
                    yield return new PresentableBlockItem<T>(i, i * _file.BlockByteSize);
                }
            }
        }

        public IEnumerable<PresentableOverfillingBlockItem<T>> OverfillingFileItems()
        {
            if (!_overfillFile.BlockMaxItemCount.HasValue)
            {
                yield break;
            }

            int index = 0;
            foreach (var block in _overfillFile.GetOverfillingBlocksSequentially())
            {
                while (index * block.ByteSize != block.Address)
                {
                    yield return new PresentableOverfillingBlockItem<T>(index, index * block.ByteSize);
                    index++;
                }

                foreach (var item in block)
                {
                    yield return new PresentableOverfillingBlockItem<T>(index, block.Address, block.Info.MainFileAddress, item);
                }
                for (int j = block.Info.ItemCount; j < block.Info.MaxItemCount; j++)
                {
                    yield return new PresentableOverfillingBlockItem<T>(index, block.Address, block.Info.MainFileAddress, default);
                }
                index++;
            }
        }

        public string GetManagingData()
        {
            string ret = "";
            ret += $"File Bit Depth: {_file.BitDepth} (Max: {_file.MaxBitDepth})\n";
            ret += "Block Addresses:\n";
            ret += string.Join(", ", _file.BlockAddresses) + "\n";
            ret += "Block Bit Depths:\n";
            ret += string.Join(", ", _file.BlockBitDepths) + "\n";
            ret += "File Block Occupation:\n";
            ret += string.Join(", ", _file.BlockOccupation);
            return ret;
        }

    }
}
