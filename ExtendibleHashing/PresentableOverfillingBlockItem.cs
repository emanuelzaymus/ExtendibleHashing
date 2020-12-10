namespace ExtendibleHashing
{
    public class PresentableOverfillingBlockItem<T>
    {
        private bool _isInvalidBlock;

        public int Index { get; }
        public int Address { get; }
        public int MainFileAddress { get; }
        public T Item { get; }

        public PresentableOverfillingBlockItem(int index, int address, int mainFileAddress, T item)
        {
            Index = index;
            Address = address;
            MainFileAddress = mainFileAddress;
            Item = item;
            _isInvalidBlock = false;
        }

        /// <summary>
        /// Empty Block
        /// </summary>
        /// <param name="index"></param>
        /// <param name="address"></param>
        public PresentableOverfillingBlockItem(int index, int address)
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
                return $"Index:{Index}; Addr:{Address}; MainFileAddr:{MainFileAddress}; {itm}";
            }
            return $"Index:{Index}; Addr:{Address}; INVALID BLOCK";
        }

    }
}
