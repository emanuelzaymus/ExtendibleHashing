using System;
using System.Collections;

namespace ExtendibleHashing.Extensions
{
    public static class BitArrayExtensions
    {
        public static int IntFromFirst(this BitArray bitArray, int nBits)
        {
            int ret = 0;
            for (int i = 0; i < nBits && i < bitArray.Length; i++)
            {
                if (bitArray[i])
                    ret += (int)Math.Pow(2, i);
            }
            return ret;
        }

    }
}
