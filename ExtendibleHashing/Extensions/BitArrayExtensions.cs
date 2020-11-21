using System;
using System.Collections;

namespace ExtendibleHashing.Extensions
{
    public static class BitArrayExtensions
    {
        public static bool[] First(this BitArray bitArray, int nBits)
        {
            bool[] ret = new bool[nBits];
            for (int i = 0; i < nBits; i++)
            {
                ret[i] = bitArray[i];
            }
            return ret;
        }

        public static int IntFromFirst(this BitArray bitArray, int nBits)
        {
            int ret = 0;
            for (int i = 0; i < nBits && i < bitArray.Length; i++)
            {
                if (bitArray[i])
                    ret += (int)Math.Pow(2, i);
            }
            return ret;

            //BitArray mask = new BitArray(BitConverter.GetBytes(Math.Pow(2, nBits) - 1));
            //bitArray.And(mask);

            //int[] array = new int[1];
            //bitArray.CopyTo(array, 0);
            //return array[0];
        }

    }
}
