using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using ExtendibleHashing.Extensions;
using System;

namespace ExtendibleHashing.Tests.Extensions
{
    [TestClass]
    public class BitArrayExtensionsTests
    {
        //[TestMethod]
        //public void IntFromFirst_NBitsIsValid_ShouldReturnRightValue()
        //{
        //    BitArray bitArray = new BitArray(new[] { 253 }); // 1111 1101
        //    int res = bitArray.IntFromNMostSignificantBits(4);
        //    Assert.AreEqual(13, res); // 0000 1101

        //    bitArray = new BitArray(new[] { 15 }); // 0000 1111
        //    res = bitArray.IntFromNMostSignificantBits(3);
        //    Assert.AreEqual(7, res);
        //}

        //[TestMethod]
        //public void IntFromFirst_NBitsIsIntLength_ShouldReturnWholeValue()
        //{
        //    BitArray bitArray = new BitArray(29); // 0001 1101
        //    int res = bitArray.IntFromNMostSignificantBits(32);
        //    Assert.AreEqual(29, res); // 0001 1101
        //}

        //[TestMethod]
        //public void IntFromFirst_NBitsIsZero_ShouldReturnZero()
        //{
        //    BitArray bitArray = new BitArray(255);
        //    int res = bitArray.IntFromNMostSignificantBits(0);
        //    Assert.AreEqual(0, res);
        //}

        //[TestMethod]
        //public void IntFromFirst_NBitsIsAboveBitArayLength_ShouldReturnZero()
        //{
        //    BitArray bitArray = new BitArray(255);
        //    Assert.ThrowsException<ArgumentOutOfRangeException>(() => bitArray.IntFromNMostSignificantBits(100));
        //}

        //[TestMethod]
        //public void IntFromFirst_NBitsIsNegative_ShouldReturnZero()
        //{
        //    BitArray bitArray = new BitArray(255);
        //    Assert.ThrowsException<ArgumentOutOfRangeException>(() => bitArray.IntFromNMostSignificantBits(-10));
        //}

        //[TestMethod]
        //public void IntFromFirst_BitArray15_ShouldReturn7()
        //{
        //    BitArray bitArray = new BitArray(new[] { true, true, true, true, false, false, false, false }); // 15 ???
        //    int res = bitArray.IntFromNMostSignificantBits(3);
        //    Assert.AreEqual(7, res);
        //}



        ///////////////////////////////////////

        [TestMethod]
        public void FirstNLeastSignificantBits_ValidValues_ShouldReturnShortenBitArray()
        {
            BitArray bitArray = new BitArray(new[] { 253 }); // 1111 1101
            BitArray actual = bitArray.FirstNLeastSignificantBits(4);
            Assert.AreEqual(13, actual.ToInt()); // 0000 1101
            Assert.AreEqual(4, actual.Length);

            bitArray = new BitArray(new[] { 15 }); // 0000 1111
            actual = bitArray.FirstNLeastSignificantBits(3);
            Assert.AreEqual(7, actual.ToInt()); // 0000 0111
            Assert.AreEqual(3, actual.Length);
        }

        [TestMethod]
        public void FirstNLeastSignificantBits_NBitsIsZero_ShouldReturnEmptyBitArray()
        {
            BitArray bitArray = new BitArray(new[] { 253 });
            BitArray actual = bitArray.FirstNLeastSignificantBits(0);
            Assert.AreEqual(0, actual.ToInt());
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void FirstNLeastSignificantBits_NBitsIsNegative_ShouldThrowException()
        {
            BitArray bitArray = new BitArray(new[] { 253 });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => bitArray.FirstNLeastSignificantBits(-1));
        }

        [TestMethod]
        public void FirstNLeastSignificantBits_NBitsIsMoreThanBitArrayLength_ShouldThrowException()
        {
            BitArray bitArray = new BitArray(new[] { 253 });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => bitArray.FirstNLeastSignificantBits(33));
        }

        [TestMethod]
        public void ReverseBits_FullValidInt_ShouldReturnReversed()
        {
            BitArray bitArray = new BitArray(new[] { 162 }); // 0000_0000 0000_0000 0000_0000 1010_0010
            int expected = 0b_01000101_00000000_00000000_00000000;
            Assert.AreEqual(expected, bitArray.ReverseBits().ToInt());
        }

        [TestMethod]
        public void ReverseBits_FullValidNegativeInt_ShouldReturnReversed()
        {
            BitArray bitArray = new BitArray(new[] { -163 });
            Assert.AreEqual(-1_157_627_905, bitArray.ReverseBits().ToInt());
        }

        [TestMethod]
        public void ReverseBits_FullValidOddInt_ShouldReturnNegativeInt()
        {
            BitArray bitArray = new BitArray(new[] { 163 });
            Assert.AreEqual(-989_855_744, bitArray.ReverseBits().ToInt());
        }

        [TestMethod]
        public void ReverseBits_EmptyBitArray_ShouldReturnZero()
        {
            BitArray bitArray = new BitArray(0);
            Assert.AreEqual(0, bitArray.ReverseBits().ToInt());
        }

        [TestMethod]
        public void ReverseBits_ShortenBitArray_ShouldReturnReversed()
        {
            BitArray bitArray = new BitArray(new[] { 245 }); // 1111 0101
            bitArray.Length = 4; // 0101
            Assert.AreEqual(10, bitArray.ReverseBits().ToInt()); // 1010
        }

        [TestMethod]
        public void ToInt_ValidInt_ShouldReturnIntValue()
        {
            BitArray bitArray = new BitArray(new[] { 253 }); // 1111 1101
            Assert.AreEqual(253, bitArray.ToInt());
        }

        [TestMethod]
        public void ToInt_NegativeInt_ShouldReturnIntValue()
        {
            BitArray bitArray = new BitArray(new[] { -253 });
            Assert.AreEqual(-253, bitArray.ToInt());
        }

        [TestMethod]
        public void ToInt_ShortenInt_ShouldReturnIntValue()
        {
            BitArray bitArray = new BitArray(new[] { 143 }); // 1000 1111
            bitArray.Length = 3; // 0000 0111
            Assert.AreEqual(7, bitArray.ToInt());
        }

        [TestMethod]
        public void ToInt_EmptyArray_ShouldReturnIntValue()
        {
            BitArray bitArray = new BitArray(0); // 0000 0000
            Assert.AreEqual(0, bitArray.ToInt());
        }

        [TestMethod]
        public void ToInt_TooLongArray_ShouldThrowException()
        {
            BitArray bitArray = new BitArray(new[] { 1, 1 }); // 0000_0000_0000_0001 0000_0000_0000_0001
            Assert.ThrowsException<ArgumentException>(() => bitArray.ToInt());
        }




    }
}
