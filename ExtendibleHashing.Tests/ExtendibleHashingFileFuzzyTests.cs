using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendibleHashing.Tests.TestClasses;
using System.Linq;
using System.Collections.Generic;
using System;

namespace ExtendibleHashing.Tests
{
    [TestClass]
    public class ExtendibleHashingFileFuzzyTests
    {
        private ExtendibleHashingFile<Town> GetExtendibleHashingFile(IEnumerable<Town> towns)
        {
            var file = ExtendibleHashingFileTests.GetExtendibleHashingFile();
            foreach (var t in towns)
            {
                file.Add(t);
            }
            return file;
        }

        [TestMethod]
        [Timeout(15000)]
        public void FuzzyAdd()
        {
            int count = 5000;
            var checkList = RandomTownGenerator.GenerateTowns(count).ToList();
            using (var f = GetExtendibleHashingFile(checkList))
            {
                CollectionAssert.AreEquivalent(checkList, f.ToList());
            }
        }

        [TestMethod]
        [Timeout(20000)]
        public void FuzzyAdd_RandomSeeds()
        {
            for (int i = 0; i < 200; i++)
            {
                RandomTownGenerator.Random = new Random(i);
                var checkList = RandomTownGenerator.GenerateTowns(40).ToList();
                using (var f = GetExtendibleHashingFile(checkList))
                {
                    CollectionAssert.AreEquivalent(checkList, f.ToList());
                }
            }
        }

        [TestMethod]
        [Timeout(20000)]
        public void FuzzyFind()
        {
            int count = 8000;
            var checkList = RandomTownGenerator.GenerateTowns(count).ToList();
            using (var f = GetExtendibleHashingFile(checkList))
            {
                foreach (var t in checkList)
                {
                    var found = f.Find(new TownId(t.Id));
                    Assert.AreEqual(t, found);
                }
            }
        }

        [TestMethod]
        [Timeout(20000)]
        public void FuzzyRemove()
        {
            int count = 8000;
            var checkList = new LinkedList<Town>(RandomTownGenerator.GenerateTowns(count));
            int i = 0;
            using (var f = GetExtendibleHashingFile(checkList))
            {
                while (checkList.Count > 0)
                {
                    Town t = checkList.Last();
                    Assert.IsTrue(f.Remove(new TownId(t.Id)));
                    Assert.IsNull(f.Find(new TownId(t.Id)));
                    checkList.RemoveLast();
                    if (i++ % 100 == 0)
                        CollectionAssert.AreEquivalent(checkList, f.ToList());
                }
            }
        }

        [TestMethod]
        [Timeout(20000)]
        public void Fuzzing()
        {
            Random r = new Random(55);

            int initCount = 700;
            var checkList = RandomTownGenerator.GenerateTowns(initCount).ToList();

            using (var file = GetExtendibleHashingFile(checkList))
            {
                Town town;
                for (int i = 0; i < 10000; i++)
                {
                    switch (r.Next(3))
                    {
                        case 0: // Add
                            town = RandomTownGenerator.GenerateTown();
                            checkList.Add(town);
                            file.Add(town);
                            Assert.AreEqual(town, file.Find(new TownId(town.Id)));

                            CollectionAssert.AreEquivalent(checkList, file.ToList());
                            break;
                        case 1: // Find
                            if (r.NextDouble() < 0.9)
                            {
                                town = checkList[r.Next(checkList.Count)];
                                Assert.AreEqual(town, file.Find(new TownId(town.Id)));
                            }
                            else
                            {
                                int randId = RandomTownGenerator.RandomId();
                                town = checkList.FirstOrDefault(x => x.Id == randId);
                                Assert.AreEqual(town, file.Find(new TownId(randId)));
                            }
                            break;
                        case 2: // Remove
                            if (r.NextDouble() < 0.9)
                            {
                                town = checkList[r.Next(checkList.Count)];
                                Assert.IsTrue(file.Remove(new TownId(town.Id)));
                                Assert.IsNull(file.Find(new TownId(town.Id)));
                                checkList.Remove(town);
                            }
                            else
                            {
                                int randId = RandomTownGenerator.RandomId();
                                town = checkList.FirstOrDefault(x => x.Id == randId);
                                if (town == null)
                                {
                                    Assert.IsFalse(file.Remove(new TownId(randId)));
                                }
                                else
                                {
                                    Assert.IsTrue(file.Remove(new TownId(randId)));
                                    checkList.RemoveAll(x => x.Id == randId);
                                }
                            }
                            CollectionAssert.AreEquivalent(checkList, file.ToList());
                            break;
                        default:
                            break;
                    }
                }
            }
        }

    }
}
