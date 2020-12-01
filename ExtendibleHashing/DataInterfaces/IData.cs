namespace ExtendibleHashing.DataInterfaces
{
    public interface IData : IBinarySerializable
    {
        bool IdEquals(object obj);
    }
}
