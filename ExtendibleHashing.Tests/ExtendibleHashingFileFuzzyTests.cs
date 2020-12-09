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

        private ExtendibleHashingFile<Town> GetExtendibleHashingFileWith3BitDepth(IEnumerable<Town> towns)
        {
            var file = ExtendibleHashingFileTests.GetExtendibleHashingFileWith3BitDepth();
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
        [Timeout(15000)]
        public void FuzzyAdd_With3BitDepth_ShoudAddToOverfillingFile()
        {
            int count = 5000;
            var checkList = RandomTownGenerator.GenerateTowns(count).ToList();
            using (var f = GetExtendibleHashingFileWith3BitDepth(checkList))
            {
                CollectionAssert.AreEquivalent(checkList, f.ToList());
            }
        }

        [TestMethod]
        [Timeout(20000)]
        public void FuzzyFind_With3BitDepth_ShoudFindInOverfillingFile()
        {
            int count = 8000;
            var checkList = RandomTownGenerator.GenerateTowns(count).ToList();
            using (var f = GetExtendibleHashingFileWith3BitDepth(checkList))
            {
                foreach (var t in checkList)
                {
                    var found = f.Find(new TownId(t.Id));
                    Assert.AreEqual(t, found);
                }
            }
        }

        [TestMethod]
        [Timeout(25000)]
        public void FuzzyRemove_With3BitDepth_ShoudRemoveFromOverfillingFile()
        {
            int count = 5000;
            var checkList = new LinkedList<Town>(RandomTownGenerator.GenerateTowns(count));
            int i = 0;
            using (var f = GetExtendibleHashingFileWith3BitDepth(checkList))
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
        public void FuzzyUpdate_With3BitDepth_ShoudUpdateFromOverfillingFile()
        {
            int count = 5000;
            var checkList = new List<Town>(RandomTownGenerator.GenerateTowns(count));
            using (var f = GetExtendibleHashingFileWith3BitDepth(checkList))
            {
                for (int i = 0; i < checkList.Count; i++)
                {
                    var t = checkList[i];
                    var newTown = new Town(t.Id, "name");
                    Assert.IsTrue(f.Update(new TownId(t.Id), newTown));
                    Assert.AreEqual(newTown, f.Find(new TownId(t.Id)));
                    checkList[i] = newTown;
                }
                CollectionAssert.AreEquivalent(checkList, f.ToList());
            }
        }

        [TestMethod]
        [Timeout(20000)]
        public void FuzzyUpdate_OnlyMainFile_ShoudUpdate()
        {
            int count = 5000;
            var checkList = new List<Town>(RandomTownGenerator.GenerateTowns(count));
            using (var f = GetExtendibleHashingFileWith3BitDepth(checkList))
            {
                for (int i = 0; i < checkList.Count; i++)
                {
                    var t = checkList[i];
                    var newTown = new Town(t.Id, "name");
                    Assert.IsTrue(f.Update(new TownId(t.Id), newTown));
                    Assert.AreEqual(newTown, f.Find(new TownId(t.Id)));
                    checkList[i] = newTown;
                }
                CollectionAssert.AreEquivalent(checkList, f.ToList());
            }
        }

        [TestMethod]
        [Timeout(20000)]
        public void Fuzzing()
        {
            int initCount = 500;
            var checkList = RandomTownGenerator.GenerateTowns(initCount).ToList();

            using (var file = GetExtendibleHashingFile(checkList))
            {
                ExecuteFuzzyingWithFile(file, checkList, 10000);
            }
        }

        [TestMethod]
        [Timeout(20000)]
        public void Fuzzing_With3BitDepth()
        {
            int initCount = 500;
            var checkList = RandomTownGenerator.GenerateTowns(initCount).ToList();

            using (var file = GetExtendibleHashingFileWith3BitDepth(checkList))
            {
                ExecuteFuzzyingWithFile(file, checkList, 10000);
            }
        }

        private void ExecuteFuzzyingWithFile(ExtendibleHashingFile<Town> file, List<Town> checkList, int iterations)
        {
            Random r = new Random(55);

            Town town;
            for (int i = 0; i < iterations; i++)
            {
                switch (r.Next(4))
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
                    case 3: // Update
                        int randIndex = r.Next(checkList.Count);
                        town = checkList[randIndex];
                        var newTown = new Town(town.Id, "new name");
                        Assert.IsTrue(file.Update(town, newTown));
                        Assert.AreEqual(newTown, file.Find(new TownId(town.Id)));
                        checkList[randIndex] = newTown;

                        CollectionAssert.AreEquivalent(checkList, file.ToList());
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
