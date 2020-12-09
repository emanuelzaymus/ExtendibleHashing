using ExtendibleHashing.DataInterfaces;

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

        internal T Find(T itemId)
        {
            foreach (var item in _items)
            {
                if (item.IdEquals(itemId))
                    return item;
            }
            return default;
        }

        internal bool Remove(T itemId)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].IdEquals(itemId))
                {
                    _items.RemoveAt(i);
                    Info.ItemCount--;
                    return true;
                }
            }
            return false;
        }

        internal bool Update(T newItem)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].IdEquals(newItem))
                {
                    _items[i] = newItem;
                    return true;
                }
            }
            return default;
        }

    }
}
