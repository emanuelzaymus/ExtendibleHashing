using System;
using System.Collections.Generic;
using System.Text;

namespace ExtendibleHashing
{
    class DataBlockManager<T> where T : IBinarySerializable, new()
    {
        private List<int> _blockAddresses = new List<int>() { 0 }; // adresar
        private List<int> _blockBitDepths = new List<int>() { 0 }; // hlbky blokov
        private List<bool> _fileBlockOccupacity = new List<bool>() { false }; // obsadenost blokov

        private readonly int _blockByteSize;

        public DataBlockManager(int blockByteSize = 4096)
        {
            _blockByteSize = blockByteSize;
        }

        public DataBlock<T> GetNewBlock()
        {
            return default;
        }

    }
}
