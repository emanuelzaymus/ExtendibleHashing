namespace ExtendibleHashing
{
    public interface IData : IBinarySerializable
    {
        bool AddressEquals(object obj);
    }
}   
