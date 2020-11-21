namespace ExtendibleHashing
{
    public interface IBinarySerializable
    {
        int ByteSize { get; }

        byte[] ToByteArray();

        void FromByteArray(byte[] byteArray, int offset);
    }
}
