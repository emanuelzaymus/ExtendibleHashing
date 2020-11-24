using System.Collections.Generic;

namespace ExtendibleHashing.Extensions
{
    public static class ListExtensions
    {
        public static List<T> DoubleValues<T>(this List<T> list)
        {
            var ret = new List<T>(list.Count * 2);
            foreach (var item in list)
            {
                ret.Add(item);
                ret.Add(item);
            }
            return ret;
        }
    }
}
