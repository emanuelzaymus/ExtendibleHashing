//using ExtendibleHashing.DataInterfaces;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ExtendibleHashing
//{
//    class DataBlockManager<T> where T : IData, new()
//    {
//        private readonly int _blockByteSize;

//        public List<bool> FileBlockOccupacity { get; } = new List<bool>() { false }; // obsadenost blokov

//        public DataBlockManager(int blockByteSize, List<bool> fileBlockOccupacity)
//        {
//            _blockByteSize = blockByteSize;
//            FileBlockOccupacity = fileBlockOccupacity;
//        }

//        public DataBlock<T> GetNewBlock(int index)
//        {
//            int indexOfEmptyBlock = FileBlockOccupacity.IndexOf(false);
//            FileBlockOccupacity[indexOfEmptyBlock] = true;
//            // If there is no more empty blocks -> Add one empty to the end.
//            if (indexOfEmptyBlock + 1 == FileBlockOccupacity.Count)
//            {
//                FileBlockOccupacity.Add(false);
//            }
//            int newAddress = indexOfEmptyBlock * _blockByteSize;
//            BlockAddresses[index] = newAddress;

//            return new DataBlock<T>(index, newAddress, _blockBitDepths[index], _blockByteSize);
//        }

//    }
//}
