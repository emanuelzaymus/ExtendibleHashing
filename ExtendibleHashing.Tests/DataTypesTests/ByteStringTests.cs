using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendibleHashing.DataTypes;
using System.Text;

namespace ExtendibleHashing.Tests.DataTypesTests
{
    [TestClass]
    public class ByteStringTests
    {
        private const int MaxLength = 20;

        private ByteString GetByteString(string str) => new ByteString(MaxLength, str);

        private ByteString GetByteString() => new ByteString(MaxLength);

        [TestMethod]
        public void ByteString_ValidParams_ShouldCreateObject()
        {
            var s = GetByteString("asdfdfgh");
            Assert.AreEqual("asdfdfgh", s.String);
        }

        [TestMethod]
        public void ByteString_Null_ShouldCreateWithNull()
        {
            var s = GetByteString(null);
            Assert.IsNull(s.String);

            s = GetByteString();
            Assert.IsNull(s.String);
        }

        [TestMethod]
        public void SetString_Null_ShouldSetNull()
        {
            var s = GetByteString();
            s.String = null;
            Assert.IsNull(s.String);
        }

        [TestMethod]
        public void SetString_ValidValue_ShouldSet()
        {
            var s = GetByteString();
            s.String = "12345678901234567890"; // 20 characters
            Assert.AreEqual("12345678901234567890", s.String);
        }

        [TestMethod]
        public void SetString_TooLongValue_ShouldThrowException()
        {
            var s = GetByteString();
            Assert.ThrowsException<ArgumentException>(() => { s.String = "123456789012345678901"; }); // 21 characters
        }

        [TestMethod]
        public void ByteSize_WithValue_ShouldReturnByteSize()
        {
            var s = GetByteString("asdfasdf");
            Assert.AreEqual(4 + 20 * 2, s.ByteSize);
        }

        [TestMethod]
        public void ByteSize_WithoutValue_ShouldReturnByteSize()
        {
            var s = GetByteString();
            Assert.AreEqual(4 + 20 * 2, s.ByteSize);
        }

        [TestMethod]
        public void ToByteArray_WithValue_ShouldReturnFilledByteArray()
        {
            var s = GetByteString("AAAABBBBCC");
            var expected = BitConverter.GetBytes(10).Concat(Encoding.Unicode.GetBytes("AAAABBBBCC---")).ToArray();
            CollectionAssert.AreEqual(expected.Take(24).ToArray(), s.ToByteArray().Take(24).ToArray());
        }

        [TestMethod]
        public void ToByteArray_WithDiacriticsValue_ShouldReturnFilledByteArray()
        {
            var s = GetByteString("ľščťžýáíéúäôÁÉÍĹĽŇŕŠ");
            var expected = BitConverter.GetBytes(20).Concat(Encoding.Unicode.GetBytes("ľščťžýáíéúäôÁÉÍĹĽŇŕŠ")).ToArray();
            CollectionAssert.AreEqual(expected, s.ToByteArray());
        }

        [TestMethod]
        public void ToByteArray_WithNull_ShouldReturnFilledByteArray()
        {
            var s = GetByteString();
            var expected = BitConverter.GetBytes(0);
            CollectionAssert.AreEqual(expected.Take(4).ToArray(), s.ToByteArray().Take(4).ToArray());
        }

        [TestMethod]
        public void FromByteArray_WithValueNoOffset_ShouldSetString()
        {
            var s = GetByteString();
            var arr = BitConverter.GetBytes(13).Concat(Encoding.Unicode.GetBytes("AAAABBŔŘŚŤÚŽŹ")).ToArray();
            s.FromByteArray(arr, 0);
            Assert.AreEqual("AAAABBŔŘŚŤÚŽŹ", s.String);
        }

        [TestMethod]
        public void FromByteArray_WithValueWithOffset_ShouldSetString()
        {
            var s = GetByteString("asd");
            var arr = new byte[150];
            byte[] data = BitConverter.GetBytes(20).Concat(Encoding.Unicode.GetBytes("AAAABBBBCCiiiii55555")).ToArray();
            Array.Copy(data, 0, arr, 75, data.Length);
            s.FromByteArray(arr, 75);
            Assert.AreEqual("AAAABBBBCCiiiii55555", s.String);
        }

        [TestMethod]
        public void FromByteArray_WithNoValue_ShouldSetEmptyString()
        {
            var s = GetByteString("asd");
            var arr = BitConverter.GetBytes(0);
            s.FromByteArray(arr, 0);
            Assert.AreEqual("", s.String);
            Assert.IsNotNull(s.String);
        }

        [TestMethod]
        public void FromByteArray_WithNotValidValue_ShouldThrowException()
        {
            var s = GetByteString();
            var arr = BitConverter.GetBytes(21).Concat(Encoding.Unicode.GetBytes("AAAABBBBCCiiiii555556666666")).ToArray();
            Assert.ThrowsException<ArgumentException>(() => s.FromByteArray(arr, 0));
        }

    }
}
