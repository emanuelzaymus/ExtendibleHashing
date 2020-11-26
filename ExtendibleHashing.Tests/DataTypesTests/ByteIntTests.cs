using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendibleHashing.DataTypes;

namespace ExtendibleHashing.Tests.DataTypesTests
{
    [TestClass]
    public class ByteIntTests
    {
        private ByteInt GetByteInt(int i) => new ByteInt(i);

        private ByteInt GetByteInt() => new ByteInt();

        [TestMethod]
        public void ByteSize_WithNumber_ShouldReturnSizeOfInt()
        {
            var i = GetByteInt(654);
            int expected = sizeof(int);
            Assert.AreEqual(expected, i.ByteSize);
        }

        [TestMethod]
        public void ByteSize_WithoutNumber_ShouldReturnSizeOfInt()
        {
            var i = GetByteInt();
            int expected = sizeof(int);
            Assert.AreEqual(expected, i.ByteSize);
        }

        [TestMethod]
        public void ToByteArray_WithNumber_ShouldReturnFilledArrayWithNumber()
        {
            var i = GetByteInt(9562487);
            byte[] expected = BitConverter.GetBytes(9562487);
            CollectionAssert.AreEqual(expected, i.ToByteArray());
        }

        [TestMethod]
        public void ToByteArray_WithoutNumber_ShouldReturnFilledArrayWith0()
        {
            var i = GetByteInt();
            byte[] expected = BitConverter.GetBytes(0);
            CollectionAssert.AreEqual(expected, i.ToByteArray());
        }

        [TestMethod]
        public void FromByteArray_OnlyNumber_ShouldSetByteInt()
        {
            var i = GetByteInt(36589);
            i.FromByteArray(BitConverter.GetBytes(15935784), 0);
            Assert.AreEqual(15935784, i.Int);
        }

        [TestMethod]
        public void FromByteArray_NumberInBiggerArray_ShouldSetByteInt()
        {
            int theNumber = 68549732;
            byte[] inputArr = new byte[150];
            Array.Copy(BitConverter.GetBytes(theNumber), 0, inputArr, 99, sizeof(int));

            var i = GetByteInt();
            i.FromByteArray(inputArr, 99);
            Assert.AreEqual(theNumber, i.Int);
        }

    }
}
