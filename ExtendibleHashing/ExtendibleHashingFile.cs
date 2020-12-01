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
        private readonly TextFileHandler _managerFile;
        private readonly DataBlockFile<T> _file;
        //private readonly FileStream _overfillFile;

        /// <summary>
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="overfillingFilePath"></param>
        /// <param name="managerFilePath"></param>
        /// <param name="blockByteSize">Block size in bytes</param>
        public ExtendibleHashingFile(string filePath, string overfillingFilePath, string managerFilePath,
            int blockByteSize = 4096, FileMode fileMode = FileMode.OpenOrCreate)
        {
            _managerFile = new TextFileHandler(managerFilePath);
            _file = new DataBlockFile<T>(filePath, fileMode, blockByteSize, _managerFile, new BitHashing());
        }

        public void Add(T item)
        {
            while (true)
            {
                var block = _file.GetDataBlock(item);

                if (block.IsFull)
                {
                    if (_file.BitDepth == block.BitDepth)
                    {
                        _file.DoubleTheFileSize();
                        block.Index *= 2;
                    }
                    _file.Split(block);
                }
                else
                {
                    block.Add(item);
                    _file.Save(block);
                    break;
                }
            }
        }

        public T Find(T itemAddress)
        {
            var block = _file.GetDataBlock(itemAddress);
            return block.Find(itemAddress);
        }

        public bool Remove(T itemAddress)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _file.SaveManagerData(_managerFile);
            _file.Dispose();
            //_overfillFile.Dispose();
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
