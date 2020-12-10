namespace ExtendibleHashing.Hashing
{
    interface IHashing
    {
        /// <summary>
        /// Hashing method.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="bitDepth"></param>
        /// <returns></returns>
        int HashCodeToIndex(int hashCode, int bitDepth);
    }
}