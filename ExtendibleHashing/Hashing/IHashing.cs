namespace ExtendibleHashing.Hashing
{
    interface IHashing
    {
        int HashCodeToIndex(int hashCode, int bitDepth);
    }
}