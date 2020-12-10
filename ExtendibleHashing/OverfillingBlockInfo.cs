namespace ExtendibleHashing
{
    /// <summary>
    /// Overfilling block information.
    /// </summary>
    class OverfillingBlockInfo
    {
        public int MaxItemCount { get; set; }

        public int MainFileAddress { get; }
        public int Address { get; }
        public int ItemCount { get; set; } = 0;
        public bool IsFull => ItemCount >= MaxItemCount;

        public OverfillingBlockInfo(int mainFileAddress, int address)
        {
            MainFileAddress = mainFileAddress;
            Address = address;
        }

        public OverfillingBlockInfo(int mainFileAddress, int address, int maxItemCount, int itemCount)
        {
            MainFileAddress = mainFileAddress;
            Address = address;
            MaxItemCount = maxItemCount;
            ItemCount = itemCount;
        }

    }
}
