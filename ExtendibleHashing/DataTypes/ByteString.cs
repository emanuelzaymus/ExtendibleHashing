using System;
using System.Text;

namespace ExtendibleHashing.DataTypes
{
    public class ByteString : IData
    {
        private const int ByteSizeOfCharacter = 2;

        private string _string;

        public int MaxLength { get; set; }

        public string String
        {
            get => _string;
            set
            {
                if (value == null || value.Length <= MaxLength)
                {
                    _string = value;
                }
                else throw new ArgumentException("New value is larger than it's MaxLength.");
            }
        }

        public ByteString(int maxLength) : this(maxLength, default) { }

        public ByteString(int maxLength, string @string)
        {
            MaxLength = maxLength;
            String = @string;
        }

        public int ByteSize => sizeof(int) + MaxLength * ByteSizeOfCharacter; // byte for actual number of valid characters + MaxLength

        public byte[] ToByteArray()
        {
            var ret = new byte[ByteSize];
            string str = String ?? "";
            Array.Copy(BitConverter.GetBytes(str.Length), 0, ret, 0, sizeof(int));// Save actual number of valid characters
            Array.Copy(Encoding.Unicode.GetBytes(str + new string('#', MaxLength - str.Length)), 0,
                ret, sizeof(int), MaxLength * ByteSizeOfCharacter);
            return ret;
        }

        public void FromByteArray(byte[] byteArray, int offset)
        {
            int actualStringLength = BitConverter.ToInt32(byteArray, offset); // Get actual number of valid characters
            String = Encoding.Unicode.GetString(byteArray, offset + sizeof(int), actualStringLength * ByteSizeOfCharacter);
        }

        public bool AddressEquals(object obj)
        {
            return obj is ByteString byteString &&
                   String == byteString.String;
        }

        public override string ToString() => String;

        public override bool Equals(object obj)
        {
            return obj is ByteString @string &&
                   String == @string.String &&
                   MaxLength == @string.MaxLength;
        }

        public override int GetHashCode()
        {
            return String != null ? String.GetHashCode() : 0;
        }

    }
}
