namespace ExtendibleHashing
{
    public interface IData : IBinarySerializable
    {
        bool IdEquals(object obj);
    }
}   
