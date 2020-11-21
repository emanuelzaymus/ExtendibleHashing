using ExtendibleHashing.DataTypes;
using System.Linq;

namespace ExtendibleHashing.ConsoleApp
{
    class Property : IBinarySerializable
    {
        private const int DescriptionLength = 20;
        private static int NextId = 1; // TODO: Need to store it externally

        private readonly ByteInt _id = new ByteInt();
        private readonly ByteInt _number = new ByteInt();
        private readonly ByteString _description = new ByteString(DescriptionLength);

        public int Id { get => _id.Int; set => _id.Int = value; }

        public int Number { get => _number.Int; set => _number.Int = value; }

        public string Description { get => _description.String; set => _description.String = value; }

        // public GpsCoordinates Coordinates1 
        // public GpsCoordinates Coordinates2

        public Property() { }

        public Property(int number, string description)
        {
            Id = NextId++;
            Number = number;
            Description = description;
        }

        protected Property(int id)
        {
            Id = id;
        }

        public int ByteSize => _id.ByteSize + _number.ByteSize + _description.ByteSize;

        public byte[] ToByteArray()
        {
            return _id.ToByteArray().Concat(_number.ToByteArray()).Concat(_description.ToByteArray()).ToArray();
        }

        public void FromByteArray(byte[] byteArray, int offset)
        {
            _id.FromByteArray(byteArray, offset);
            _number.FromByteArray(byteArray, offset + _id.ByteSize);
            _description.FromByteArray(byteArray, offset + _id.ByteSize + _number.ByteSize);
        }

        public override string ToString()
        {
            return $"Id:{Id}, Number:{Number}, Desc:{Description}";
        }

        public override bool Equals(object obj)
        {
            return obj is Property property &&
                Id == property.Id;
        }

        public override int GetHashCode() => Id;
    }
}
