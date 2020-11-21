using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using ExtendibleHashing.Extensions;

namespace ExtendibleHashing.Tests.Extensions
{
    [TestClass]
    public class BitArrayExtensionsTests
    {
        [TestMethod]
        public void IntFromFirst_NBitsIsValid_ShouldReturnRightValue()
        {
            BitArray bitArray = new BitArray(new[] { true, false, true, true, true });
            int res = bitArray.IntFromFirst(4);
            Assert.AreEqual(13, res);
        }

        [TestMethod]
        public void IntFromFirst_NBitsIsAboveBitArrayLength_ShouldReturnWholeValue()
        {
            BitArray bitArray = new BitArray(new[] { true, false, true, true, true });
            int res = bitArray.IntFromFirst(100);
            Assert.AreEqual(29, res);
        }

        [TestMethod]
        public void IntFromFirst_NBitsIsZero_ShouldReturnZero()
        {
            BitArray bitArray = new BitArray(new[] { true, false, true, true, true });
            int res = bitArray.IntFromFirst(0);
            Assert.AreEqual(0, res);
        }

        [TestMethod]
        public void IntFromFirst_NBitsIsNegative_ShouldReturnZero()
        {
            BitArray bitArray = new BitArray(new[] { true, false, true, true, true });
            int res = bitArray.IntFromFirst(-10);
            Assert.AreEqual(0, res);
        }
    }
}
