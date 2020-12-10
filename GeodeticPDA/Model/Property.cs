using ExtendibleHashing.DataInterfaces;
using ExtendibleHashing.DataTypes;
using System;

namespace GeodeticPDA.Model
{
    class Property : IData
    {
        private const int DescriptionLength = 20;
        private static int NextId = 1; // TODO: Need to store it externally

        private readonly ByteInt _id = new ByteInt();
        private readonly ByteInt _number = new ByteInt();
        private readonly ByteString _description = new ByteString(DescriptionLength);

        public int Id { get => _id.Int; set => _id.Int = value; }

        public int Number { get => _number.Int; set => _number.Int = value; }

        public string Description { get => _description.String; set => _description.String = value; }

        public GpsCoordinates GpsCoordinates1 { get; } = new GpsCoordinates();

        public GpsCoordinates GpsCoordinates2 { get; } = new GpsCoordinates();

        public Property() { }

        public Property(int id, int number, string description, GpsCoordinates gpsCoordinates1, GpsCoordinates gpsCoordinates2)
        {
            if (id >= NextId)
            {
                NextId = id + 1;
            }
            Id = id;
            Number = number;
            Description = description;
            GpsCoordinates1 = gpsCoordinates1;
            GpsCoordinates2 = gpsCoordinates2;
        }

        public Property(int number, string description, GpsCoordinates gpsCoordinates1, GpsCoordinates gpsCoordinates2)
            : this(NextId++, number, description, gpsCoordinates1, gpsCoordinates2)
        {
        }

        protected Property(int id)
        {
            Id = id;
        }

        // 4 + 4 + 44 + 16 + 16 = 84
        public int ByteSize => _id.ByteSize + _number.ByteSize + _description.ByteSize + GpsCoordinates1.ByteSize + GpsCoordinates2.ByteSize;

        public byte[] ToByteArray()
        {
            var ret = new byte[ByteSize];
            int destIndex = 0;
            Array.Copy(_id.ToByteArray(), 0, ret, destIndex, _id.ByteSize);
            Array.Copy(_number.ToByteArray(), 0, ret, destIndex += _id.ByteSize, _number.ByteSize);
            Array.Copy(_description.ToByteArray(), 0, ret, destIndex += _number.ByteSize, _description.ByteSize);
            Array.Copy(GpsCoordinates1.ToByteArray(), 0, ret, destIndex += _description.ByteSize, GpsCoordinates1.ByteSize);
            Array.Copy(GpsCoordinates2.ToByteArray(), 0, ret, destIndex += GpsCoordinates1.ByteSize, GpsCoordinates2.ByteSize);
            return ret;
        }

        public void FromByteArray(byte[] byteArray, int offset)
        {
            _id.FromByteArray(byteArray, offset);
            _number.FromByteArray(byteArray, offset += _id.ByteSize);
            _description.FromByteArray(byteArray, offset += _number.ByteSize);
            GpsCoordinates1.FromByteArray(byteArray, offset += _description.ByteSize);
            GpsCoordinates2.FromByteArray(byteArray, offset += GpsCoordinates1.ByteSize);
        }

        public bool IdEquals(object obj)
        {
            return obj is Property property &&
                Id == property.Id;
        }

        public override string ToString()
        {
            return $"Id:{Id}, Numb:{Number}, Desc:{Description}, Gps1:{GpsCoordinates1}, Gps2:{GpsCoordinates2}";
        }

        public override bool Equals(object obj)
        {
            return obj is Property property &&
                   Id == property.Id &&
                   Number == property.Number &&
                   Description == property.Description &&
                   GpsCoordinates1.Equals(property.GpsCoordinates1) &&
                   GpsCoordinates2.Equals(property.GpsCoordinates2);
        }

        public override int GetHashCode() => Id;

    }
}
