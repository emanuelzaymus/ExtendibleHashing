namespace ExtendibleHashing.DataInterfaces
{
    public interface IBinarySerializable
    {
        /// <summary>
        /// Size of object in bytes.
        /// </summary>
        int ByteSize { get; }

        /// <summary>
        /// Returns byte representation of the object of size no more than ByteSize.
        /// </summary>
        /// <returns></returns>
        byte[] ToByteArray();

        /// <summary>
        /// Loads the object from byte representation.
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="offset"></param>
        void FromByteArray(byte[] byteArray, int offset);
    }
}
