using GeodeticPDA.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeodeticPDA.DataGeneration
{
    class RandomPropertyGenerator
    {
        private static readonly Random Rand = new Random(1);

        public static IEnumerable<Property> GenerateProperties(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return GenerateProperty();
            }
        }

        private static Property GenerateProperty()
        {
            return new Property(Rand.Next(), RandomString(), RandomGpsCoordinates(), RandomGpsCoordinates());
        }

        private static GpsCoordinates RandomGpsCoordinates()
        {
            return new GpsCoordinates(RandomDouble(), RandomDouble());
        }

        private static string RandomString()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, Rand.Next(5, 15)).Select(s => s[Rand.Next(s.Length)]).ToArray());
        }

        private static double RandomDouble() => Rand.NextDouble() * Rand.Next(-10000, 10000);

    }
}
