using ExtendibleHashing.Extensions;
using System;
using System.Collections;

namespace ExtendibleHashing.Hashing
{
    class BitHashing : IHashing
    {
        private const int MaxBitDepth = sizeof(int) * 8; // 4 * 8 = 32

        /// <summary>
        /// Returns number from first <paramref name="bitDepth"/> least significant bits - reversed.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="bitDepth"></param>
        /// <returns></returns>
        public int HashCodeToIndex(int hashCode, int bitDepth)
        {
            if (bitDepth > MaxBitDepth)
                throw new ArgumentOutOfRangeException(nameof(bitDepth),
                    $"Bit depth must be greater or equal 0 and less or equal {MaxBitDepth}.");

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
