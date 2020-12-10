namespace ExtendibleHashing
{
    public class PresentableBlockItem<T>
    {
        private bool _isInvalidBlock;

        public int Index { get; }

        public int Address { get; }

        public int BitDepth { get; }

        public T Item { get; }

        public PresentableBlockItem(int index, int address, int bitDepth, T item)
        {
            Index = index;
            Address = address;
            BitDepth = bitDepth;
            Item = item;
            _isInvalidBlock = false;
        }

        public PresentableBlockItem(int index, int address, T item)
            : this(index, address, -1, item)
        {
        }

        /// <summary>
        /// Empty Block
        /// </summary>
        /// <param name="index"></param>
        /// <param name="address"></param>
        public PresentableBlockItem(int index, int address)
        {
            Index = index;
            Address = address;
            _isInvalidBlock = true;
        }

        public override string ToString()
        {
            if (!_isInvalidBlock)
            {
                string itm = "XXX";
                if (Item != null)
                {
                    itm = Item.ToString();
                }
                if (BitDepth > 0)
                {
                    return $"Index:{Index}; Addr:{Address}; BitDepth:{BitDepth}; {itm}";
                }
                return $"Index:{Index}; Addr:{Address}; {itm}";
            }
            return $"Index:{Index}; Addr:{Address}; INVALID BLOCK";
        }

    }
}
