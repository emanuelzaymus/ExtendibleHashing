using ExtendibleHashing.DataInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ExtendibleHashing
{
    class OverfillingBlock<T> : AbstractBlock<T> where T : IData, new()
    {
        public OverfillingBlockInfo Info { get; }

        public OverfillingBlock(int blockByteSize, OverfillingBlockInfo info) : base(info.Address, blockByteSize)
        {
            Info = info;
            Info.MaxItemCount = base.MaxItemCount;
        }

        public OverfillingBlock(int blockByteSize, OverfillingBlockInfo blockInfo, byte[] data) : this(blockByteSize, blockInfo)
        {
            base.FromByteArray(data, 0);
        }

        internal void Add(T item)
        {
            _items.Add(item);
            Info.ItemCount++;
        }
    }
}
