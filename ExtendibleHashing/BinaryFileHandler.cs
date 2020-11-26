//using System;
//using System.IO;

//namespace ExtendibleHashing
//{
//    class BinaryFileHandler : IDisposable
//    {
//        private readonly FileStream _file;
//        private readonly int _blockByteSize;

//        public BinaryFileHandler(string filePath, FileMode fileMode, int blockByteSize)
//        {
//            _file = new FileStream(filePath, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite, blockByteSize, FileOptions.WriteThrough);
//            _blockByteSize = blockByteSize;

//            if (_file.Length < _blockByteSize)
//            {
//                Save(new byte[_blockByteSize], 0);
//            }
//        }

//        public byte[] ReadBlock(int address)
//        {
//            _file.Seek(address, SeekOrigin.Begin);
//            byte[] block = new byte[_blockByteSize];
//            _file.Read(block, 0, _blockByteSize);
//            return block;
//        }

//        public void Save(byte[] array, int offest)
//        {
//            _file.Seek(offest, SeekOrigin.Begin);
//            _file.Write(array, 0, _blockByteSize);
//            _file.Flush(true);
//        }

//        //private void Save(DataBlock<T> block)
//        //{
//        //    _file.Seek(block.InFileAddress, SeekOrigin.Begin);
//        //    _file.Write(block.ToByteArray(), 0, block.ByteSize);
//        //    _file.Flush(true);
//        //}

//        public void Dispose()
//        {
//            _file.Dispose();
//        }
//    }
//}
