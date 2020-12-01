using ExtendibleHashing.Extensions;
using System;
using System.Collections;

namespace ExtendibleHashing.Hashing
{
    class BitHashing : IHashing
    {
        public int HashCodeToIndex(int hashCode, int bitDepth)
        {
            BitArray bits = HashCodeToBitArray(hashCode);
            BitArray firstNBits = bits.FirstNLeastSignificantBits(bitDepth);
            BitArray reversed = firstNBits.ReverseBits();
            return reversed.ToInt();
        }

        private BitArray HashCodeToBitArray(int hashCode)
        {
            return new BitArray(BitConverter.GetBytes(hashCode));
        }
    }
}
