using System;
using System.Collections;

namespace ExtendibleHashing.Extensions
{
    public static class BitArrayExtensions
    {

        public static BitArray FirstNLeastSignificantBits(this BitArray bitArray, int nBits)
        {
            if (nBits >= bitArray.Length || nBits < 0)
                throw new ArgumentOutOfRangeException(nameof(nBits));

            BitArray ret = new BitArray(nBits);
            for (int i = 0; i < nBits && i < bitArray.Length; i++)
            {
                ret[i] = bitArray[i];
            }
            return ret;
        }

        public static BitArray ReverseBits(this BitArray bitArray)
        {
            int len = bitArray.Length;
            BitArray reversed = new BitArray(len);
            for (int i = 0; i < len; i++)
            {
                reversed[len - i - 1] = bitArray[i];
            }
            return reversed;
        }

        public static int ToInt(this BitArray bitArray)
        {
            if (bitArray.Length > 32)
                throw new ArgumentException($"Length of '{nameof(bitArray)}' is more than 32.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];
        }

    }
}
