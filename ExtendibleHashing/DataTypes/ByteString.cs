using System;
using System.Text;
using System.Linq;

namespace ExtendibleHashing.DataTypes
{
    public class ByteString : IBinarySerializable
    {
        public string String
        {
            get => String;
            set
            {
                if (value.Length <= MaxLength)
                {
                    String = value;
                }
                throw new ArgumentException("New value is larger than it's Length.");
            }
        }

        public int MaxLength
        {
            get => MaxLength;
            set
            {
                if (MaxLength <= sizeof(byte))
                {
                    MaxLength = value;
                }
                throw new ArgumentException($"New {nameof(MaxLength)} is more than 255.");
            }
        }

        public ByteString(int maxLength) : this(default, maxLength) { }

        public ByteString(string @string, int maxLength)
        {
            String = @string;
            MaxLength = maxLength;
        }

        public int ByteSize => sizeof(byte) + MaxLength; // byte for actual number of valid characters + MaxLength

        public byte[] ToByteArray()
        {
            return (new byte[] { (byte)String.Length }) // Save actual number of valid characters
                .Concat(Encoding.ASCII.GetBytes(String + new string('#', MaxLength - String.Length)))
                .ToArray();
        }

        public void FromByteArray(byte[] byteArray, int offset)
        {
            byte actualStringLength = byteArray[offset]; // Get actual number of valid characters
            String = Encoding.ASCII.GetString(byteArray, offset + 1, actualStringLength);
        }
        public override string ToString() => String;

        public override bool Equals(object obj)
        {
            return obj is ByteString @string &&
                   String == @string.String;
        }

        public override int GetHashCode() => String.GetHashCode();
    }
}
