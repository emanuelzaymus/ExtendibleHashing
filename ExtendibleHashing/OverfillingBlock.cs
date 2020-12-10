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

        /// <summary>
        /// Adds item to the block.
        /// </summary>
        /// <param name="item"></param>
        internal void Add(T item)
        {
            _items.Add(item);
            Info.ItemCount++;
        }

        /// <summary>
        /// Finds item based on <paramref name="itemId"/>.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        internal T Find(T itemId)
        {
            foreach (var item in _items)
            {
                if (item.IdEquals(itemId))
                    return item;
            }
            return default;
        }

        /// <summary>
        /// Removes from block.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns>Success</returns>
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

        /// <summary>
        /// Updates <paramref name="newItem"/> if exists in the block.
        /// </summary>
        /// <param name="newItem"></param>
        /// <returns>Success</returns>
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
