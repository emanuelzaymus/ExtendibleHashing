using ExtendibleHashing.DataInterfaces;
using ExtendibleHashing.DataTypes;
using System;

namespace GeodeticPDA.Model
{
    class GpsCoordinates : IBinarySerializable
    {
        private ByteDouble _latitude = new ByteDouble();
        private ByteDouble _longitude = new ByteDouble();

        public double Latitude { get => _latitude.Double; set => _latitude.Double = value; }
        public double Longitude { get => _longitude.Double; set => _longitude.Double = value; }

        public int ByteSize => _latitude.ByteSize + _longitude.ByteSize; // 8 + 8 = 16

        public GpsCoordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public GpsCoordinates()
        {
        }

        public byte[] ToByteArray()
        {
            var ret = new byte[ByteSize];
            Array.Copy(_latitude.ToByteArray(), 0, ret, 0, _latitude.ByteSize);
            Array.Copy(_longitude.ToByteArray(), 0, ret, _latitude.ByteSize, _longitude.ByteSize);
            return ret;
        }

        public void FromByteArray(byte[] byteArray, int offset)
        {
            _latitude.FromByteArray(byteArray, offset);
            _longitude.FromByteArray(byteArray, offset + _latitude.ByteSize);
        }

        public override string ToString()
        {
            return $"Lat:{Latitude}, Long:{Longitude}";
        }

        public override bool Equals(object obj)
        {
            return obj is GpsCoordinates coordinates &&
                   Latitude == coordinates.Latitude &&
                   Longitude == coordinates.Longitude;
        }

        public override int GetHashCode() => _latitude.GetHashCode() + _longitude.GetHashCode();

    }
}
