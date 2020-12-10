namespace ExtendibleHashing.DataInterfaces
{
    public interface IData : IBinarySerializable
    {
        /// <summary>
        /// Checks equality of all id attributes of two data objects.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool IdEquals(object obj);
    }
}
