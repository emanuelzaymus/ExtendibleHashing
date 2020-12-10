using ExtendibleHashing.DataInterfaces;
using System;

namespace ExtendibleHashing.DataTypes
{
    public class ByteInt : IData
    {
        public int Int { get; set; }

        public ByteInt() : this(default) { }

        public ByteInt(int @int)
        {
            Int = @int;
        }

        public int ByteSize => sizeof(int); // 4

        public byte[] ToByteArray() => BitConverter.GetBytes(Int);

        public void FromByteArray(byte[] byteArray, int offset)
        {
            Int = BitConverter.ToInt32(byteArray, offset);
        }

        public bool IdEquals(object obj)
        {
            return obj is ByteInt byteInt &&
                   Int == byteInt.Int;
        }

        public override string ToString() => Int.ToString();

        public override bool Equals(object obj)
        {
            return obj is ByteInt @int &&
                   Int == @int.Int;
        }

        public override int GetHashCode() => Int.GetHashCode();

    }
}
