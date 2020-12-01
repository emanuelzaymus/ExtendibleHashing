using ExtendibleHashing.DataTypes;
using System;

namespace ExtendibleHashing.Tests.TestClasses
{
    public class Town : IData
    {
        private ByteInt _id = new ByteInt();
        private ByteString _name = new ByteString(20);

        public int Id { get => _id.Int; set => _id.Int = value; }
        public string Name { get => _name.String; set => _name.String = value; }

        public Town(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public Town() { }

        public int ByteSize => _id.ByteSize + _name.ByteSize;

        public byte[] ToByteArray()
        {
            var ret = new byte[ByteSize];
            Array.Copy(_id.ToByteArray(), 0, ret, 0, _id.ByteSize);
            Array.Copy(_name.ToByteArray(), 0, ret, _id.ByteSize, _name.ByteSize);
            return ret;
        }

        public void FromByteArray(byte[] byteArray, int offset)
        {
            _id.FromByteArray(byteArray, offset);
            _name.FromByteArray(byteArray, offset + _id.ByteSize);
        }

        public bool IdEquals(object obj)
        {
            return obj is Town town &&
                Id == town.Id;
        }

        public override string ToString()
        {
            return $"Id:{Id}, Name:{Name}";
        }

        public override bool Equals(object obj)
        {
            return obj is Town town &&
                   Id == town.Id &&
                   Name == town.Name;
        }

        public override int GetHashCode() => Id;

    }
}
