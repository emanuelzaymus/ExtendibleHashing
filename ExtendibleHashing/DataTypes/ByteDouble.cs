using ExtendibleHashing.DataInterfaces;
using System;

namespace ExtendibleHashing.DataTypes
{
    public class ByteDouble : IData
    {
        public double Double { get; set; }

        public ByteDouble() : this(default) { }

        public ByteDouble(double @double)
        {
            Double = @double;
        }

        public int ByteSize => sizeof(double); // 8

        public byte[] ToByteArray() => BitConverter.GetBytes(Double);

        public void FromByteArray(byte[] byteArray, int offset)
        {
            Double = BitConverter.ToDouble(byteArray, offset);
        }

        public bool IdEquals(object obj)
        {
            return obj is ByteDouble byteDouble &&
                   Double == byteDouble.Double;
        }

        public override string ToString() => Double.ToString();

        public override bool Equals(object obj)
        {
            return obj is ByteDouble @double &&
                   Double == @double.Double;
        }

        public override int GetHashCode() => Double.GetHashCode();

    }
}
