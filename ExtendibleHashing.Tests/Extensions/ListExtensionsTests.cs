using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendibleHashing.Extensions;
using System.Collections.Generic;

namespace ExtendibleHashing.Tests.Extensions
{
    [TestClass]
    public class ListExtensionsTests
    {
        [TestMethod]
        public void DoubleValues_ValidList_ShouldReturnListWithDoubledValues()
        {
            var l = new List<int>() { 54, 5, 8, 9, 2 };
            var expected = new List<int>() { 54, 54, 5, 5, 8, 8, 9, 9, 2, 2 };
            CollectionAssert.AreEqual(expected, l.DoubleValues());
        }

        [TestMethod]
        public void DoubleValues_EmptyList_ShouldReturnEmptyList()
        {
            var l = new List<int>();
            var expected = new List<int>();
            CollectionAssert.AreEqual(expected, l.DoubleValues());
        }

        [TestMethod]
        public void RemoveEveryOtherValue_ValidList_ShouldReturnModifiedList()
        {
            var l = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var expected = new List<int>() { 0, 2, 4, 6, 8 };
            CollectionAssert.AreEqual(expected, l.RemoveEveryOtherValue());
        }

        [TestMethod]
        public void RemoveEveryOtherValue_EmptyList_ShouldReturnEmptyList()
        {
            var l = new List<int>();
            var expected = new List<int>();
            CollectionAssert.AreEqual(expected, l.RemoveEveryOtherValue());
        }

    }
}
