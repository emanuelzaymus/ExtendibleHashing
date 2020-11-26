using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtendibleHashing.Tests.TestClasses
{
    public class RandomTownGenerator
    {
        public static Random Random { get; set; } = new Random(1);

        public static Town GenerateTown()
        {
            return new Town(RandomId(), RandomName());
        }

        public static IEnumerable<Town> GenerateTowns(int count)
        {
            for (int i = 0; i < count; i++)
                yield return GenerateTown();
        }

        public static int RandomId() => RandomInt();

        private static string RandomName() => RandomString(Random.Next(15, 21));

        private static int RandomInt() => Random.Next(int.MinValue, int.MaxValue);

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

    }
}
