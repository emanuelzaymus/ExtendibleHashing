using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendibleHashing.Tests.TestClasses;
using System.Linq;
using System.IO;
using System;

namespace ExtendibleHashing.Tests
{
    [TestClass]
    public class ExtendibleHashingFileTests
    {
        private const string FilePath = "test_extendibleHashingFile.bin";
        private const string OverfillingFilePath = "test_overfillingFile.bin";
        private const string ManagerFilePath = "test_managerFile.txt";
        private const string OverfillingManagerFilePath = "test_overfillingManagerFile.txt";
        private const int BlockByteSize = 256; // (Town.ByteSize = 48) * 5
        private const int OverfillingBlockByteSize = 256;

        public static ExtendibleHashingFile<Town> GetExtendibleHashingFile()
        {
            return new ExtendibleHashingFile<Town>(FilePath, OverfillingFilePath, ManagerFilePath,
                OverfillingManagerFilePath, BlockByteSize, OverfillingBlockByteSize, FileMode.Create);
        }

        public static ExtendibleHashingFile<Town> GetExtendibleHashingFileWith3BitDepth()
        {
            return new ExtendibleHashingFile<Town>(FilePath, OverfillingFilePath, ManagerFilePath,
                OverfillingManagerFilePath, BlockByteSize, OverfillingBlockByteSize, FileMode.Create, 3);
        }

        private ExtendibleHashingFile<Town> GetExtendibleHashingFileFilled()
        {
            return Fill(GetExtendibleHashingFile());
        }

        public static ExtendibleHashingFile<Town> GetExtendibleHashingFileWith3BitDepthFilled()
        {
            return FillUpOneBlock(GetExtendibleHashingFileWith3BitDepth());
        }

        private static ExtendibleHashingFile<Town> Fill(ExtendibleHashingFile<Town> f)
        {
            f.Add(new Town(0b_0000_0000, "Žilina"));
            f.Add(new Town(0b_0010_0110, "Košice"));
            f.Add(new Town(0b_1010_1001, "Martin"));
            f.Add(new Town(0b_1101_1101, "Levice"));
            f.Add(new Town(0b_1010_0101, "Trnava"));
            f.Add(new Town(0b_0110_1101, "Snina"));
            f.Add(new Town(0b_0000_0101, "Senica"));
            f.Add(new Town(0b_1000_0000, "Poprad"));
            f.Add(new Town(0b_1010_0110, "Lučenec"));
            f.Add(new Town(0b_0011_0110, "Nitra"));
            f.Add(new Town(0b_1001_0111, "Zvolen"));
            f.Add(new Town(0b_0000_1111, "Prešov"));
            f.Add(new Town(0b_1110_1101, "Púchov"));
            f.Add(new Town(0b_1111_0000, "Ilava"));
            f.Add(new Town(0b_0011_1100, "Brezno"));
            return f;
        }

        private static ExtendibleHashingFile<Town> FillUpOneBlock(ExtendibleHashingFile<Town> f)
        {
            f.Add(new Town(0b_1101_1101, "Levice"));
            f.Add(new Town(0b_1010_0101, "Trnava"));
            f.Add(new Town(0b_0110_1101, "Snina"));
            f.Add(new Town(0b_0000_0101, "Senica"));
            f.Add(new Town(0b_1110_1101, "Púchov"));
            return f;
        }

        [TestMethod]
        public void LoadingFiles_ValidFiles_ShouldLoadAllDataFromFiles()
        {
            var f = GetExtendibleHashingFileFilled();
            f.Dispose();
            using (f = new ExtendibleHashingFile<Town>(FilePath, OverfillingFilePath, ManagerFilePath, OverfillingManagerFilePath))
            {
                var expected = new[] {
                    "Žilina", "Poprad", "Ilava", "Brezno",
                    "Košice", "Lučenec", "Nitra",
                    "Martin",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Zvolen", "Prešov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                f.Add(new Town(0b0000_1110, "My Town 1")); // address: 011
                f.Add(new Town(0b1000_1110, "My Town 2")); // address: 011
                expected = new[] {
                    "Žilina", "Poprad", "Ilava", "Brezno",
                    "Košice", "Lučenec", "Nitra", "My Town 1", "My Town 2",
                    "Martin",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Zvolen", "Prešov"
                };
                actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Add_OneValue_ShouldAdd()
        {
            using (var f = GetExtendibleHashingFile())
            {
                Town t = new Town(14, "Zilina");
                f.Add(t);
                CollectionAssert.AreEqual(new[] { t }, f.ToArray());
            }
        }

        [TestMethod]
        public void Add_LectureValues_ShouldAddToRightBlocksInTheRightOrder()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                var expected = new[] {
                    "Žilina", "Poprad", "Ilava", "Brezno",
                    "Košice", "Lučenec", "Nitra",
                    "Martin",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Zvolen", "Prešov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }


        [TestMethod]
        public void Add_MoreValue_ShouldAddToRightBlocksInTheRightOrder()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                f.Add(new Town(0b0000_1110, "My Town 1")); // address: 011
                f.Add(new Town(0b1000_1110, "My Town 2")); // address: 011
                var expected = new[] {
                    "Žilina", "Poprad", "Ilava", "Brezno",
                    "Košice", "Lučenec", "Nitra", "My Town 1", "My Town 2",
                    "Martin",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Zvolen", "Prešov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                // Double the size
                f.Add(new Town(0b1000_1110, "My Town 3")); // address: 0111
                f.Add(new Town(0b1000_0110, "My Town 4")); // address: 0110
                expected = new[] {
                    "Žilina", "Poprad", "Ilava", "Brezno",
                    "Košice", "Lučenec", "Nitra", "My Town 4",
                    "My Town 1", "My Town 2", "My Town 3",
                    "Martin",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Zvolen", "Prešov"
                };
                actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Find_LectureValues_ShouldFindAllOfThem()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                Town town;
                town = f.Find(new TownId(0)); // 0b_0000_0000
                Assert.AreEqual(new Town(0, "Žilina"), town);
                town = f.Find(new TownId(38)); // 0b_0010_0110
                Assert.AreEqual(new Town(38, "Košice"), town);
                town = f.Find(new TownId(169)); // 0b_1010_1001
                Assert.AreEqual(new Town(169, "Martin"), town);
                town = f.Find(new TownId(221)); // 0b_1101_1101
                Assert.AreEqual(new Town(221, "Levice"), town);
                town = f.Find(new TownId(0b_1010_0101));
                Assert.AreEqual(new Town(0b_1010_0101, "Trnava"), town);
                town = f.Find(new TownId(0b_0110_1101));
                Assert.AreEqual(new Town(0b_0110_1101, "Snina"), town);
                town = f.Find(new TownId(0b_0000_0101));
                Assert.AreEqual(new Town(0b_0000_0101, "Senica"), town);
                town = f.Find(new TownId(0b_1000_0000));
                Assert.AreEqual(new Town(0b_1000_0000, "Poprad"), town);
                town = f.Find(new TownId(0b_1010_0110));
                Assert.AreEqual(new Town(0b_1010_0110, "Lučenec"), town);
                town = f.Find(new TownId(0b_0011_0110));
                Assert.AreEqual(new Town(0b_0011_0110, "Nitra"), town);
                town = f.Find(new TownId(0b_1001_0111));
                Assert.AreEqual(new Town(0b_1001_0111, "Zvolen"), town);
                town = f.Find(new TownId(0b_0000_1111));
                Assert.AreEqual(new Town(0b_0000_1111, "Prešov"), town);
                town = f.Find(new TownId(0b_1110_1101));
                Assert.AreEqual(new Town(0b_1110_1101, "Púchov"), town);
                town = f.Find(new TownId(0b_1111_0000));
                Assert.AreEqual(new Town(0b_1111_0000, "Ilava"), town);
                town = f.Find(new TownId(0b_0011_1100));
                Assert.AreEqual(new Town(0b_0011_1100, "Brezno"), town);
            }
        }

        [TestMethod]
        public void Remove_OneTown_SholudRemoveWithouotMerging()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                Assert.IsTrue(f.Remove(new TownId(0b_1000_0000))); // "Poprad"
                Assert.IsNull(f.Find(new TownId(0b_1000_0000)));

                var expected = new[] {
                    "Žilina", "Ilava", "Brezno",
                    "Košice", "Lučenec", "Nitra",
                    "Martin",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Zvolen", "Prešov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_TownFromLeftBlockWithBithDepth3_SholudRemoveWithMergingOneRightBlock()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                Assert.IsTrue(f.Remove(new TownId(169))); // "Martin"
                Assert.IsNull(f.Find(new TownId(169)));

                var expected = new[] {
                    "Žilina", "Poprad", "Ilava", "Brezno",
                    "Košice", "Lučenec", "Nitra",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Zvolen", "Prešov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_TownFromRightBlockWithBithDepth3_SholudRemoveWithMergingOneLeftBlock()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                Assert.IsTrue(f.Remove(new TownId(0b_0110_1101))); // "Snina"
                Assert.IsNull(f.Find(new TownId(0b_0110_1101)));

                var expected = new[] {
                    "Žilina", "Poprad", "Ilava", "Brezno",
                    "Košice", "Lučenec", "Nitra",
                    "Martin", "Levice", "Trnava", "Senica", "Púchov",
                    "Zvolen", "Prešov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_TownFromLeftBlockWithBithDepth2_SholudRemoveWithMergingTwoRightBlocks()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long fileLength = file.Length;

                Assert.IsTrue(f.Remove(new TownId(0b_1000_0000))); // "Poprad"
                Assert.IsNull(f.Find(new TownId(0b_1000_0000)));
                Assert.AreEqual(fileLength, file.Length);

                Assert.IsTrue(f.Remove(new TownId(0b_1111_0000))); // "Ilava"
                Assert.IsNull(f.Find(new TownId(0b_1111_0000)));
                Assert.AreEqual(fileLength - BlockByteSize, file.Length);

                var expected = new[] {
                    "Žilina", "Brezno", "Košice", "Lučenec", "Nitra",
                    "Martin",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Zvolen", "Prešov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_TownFromRightBlockWithBithDepth2_SholudRemoveWithMergingTwoLefttBlocks()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long fileLength = file.Length;

                Assert.IsTrue(f.Remove(new TownId(38))); // "Košice"
                Assert.IsNull(f.Find(new TownId(38)));
                Assert.AreEqual(fileLength, file.Length);

                Assert.IsTrue(f.Remove(new TownId(0b_0011_0110))); // "Nitra"
                Assert.IsNull(f.Find(new TownId(0b_0011_0110)));
                Assert.AreEqual(fileLength - BlockByteSize, file.Length);

                var expected = new[] {
                    "Žilina", "Poprad", "Ilava", "Brezno", "Lučenec",
                    "Martin",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Zvolen", "Prešov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_AllItemsFromBlockNotHavingNeighbour_SholudRemoveWithoutMergingAndLeaveItEmpty()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                Assert.IsTrue(f.Remove(new TownId(0b_1001_0111))); // "Zvolen"
                Assert.IsNull(f.Find(new TownId(0b_1001_0111)));

                Assert.IsTrue(f.Remove(new TownId(0b_0000_1111))); // "Prešov"
                Assert.IsNull(f.Find(new TownId(0b_0000_1111)));

                var expected = new[] {
                    "Žilina", "Poprad", "Ilava", "Brezno",
                    "Košice", "Lučenec", "Nitra",
                    "Martin",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_Items_SholudRemoveWithoutMergingAndLeaveItEmpty()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                Assert.IsTrue(f.Remove(new TownId(0))); // "Žilina"
                Assert.IsTrue(f.Remove(new TownId(0b_1000_0000))); // "Poprad"

                Assert.IsTrue(f.Remove(new TownId(0b_1001_0111))); // "Zvolen"
                Assert.IsTrue(f.Remove(new TownId(0b_0000_1111))); // "Prešov"

                Assert.IsTrue(f.Remove(new TownId(221))); // "Levice"
                Assert.AreEqual(BlockByteSize * 2, file.Length);

                var expected = new[] {
                    "Ilava", "Brezno", "Košice", "Lučenec", "Nitra",
                    "Martin", "Trnava", "Snina", "Senica", "Púchov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_AllItems_SholudRemoveAllItems()
        {
            using (var f = GetExtendibleHashingFileFilled())
            {
                Assert.IsTrue(f.Remove(new TownId(0)));
                Assert.IsTrue(f.Remove(new TownId(38)));
                Assert.IsTrue(f.Remove(new TownId(169)));
                Assert.IsTrue(f.Remove(new TownId(221)));
                Assert.IsTrue(f.Remove(new TownId(0b_1010_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_0110_1101)));
                Assert.IsTrue(f.Remove(new TownId(0b_0000_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_1000_0000)));
                Assert.IsTrue(f.Remove(new TownId(0b_1010_0110)));
                Assert.IsTrue(f.Remove(new TownId(0b_0011_0110)));
                Assert.IsTrue(f.Remove(new TownId(0b_1001_0111)));
                Assert.IsTrue(f.Remove(new TownId(0b_0000_1111)));
                Assert.IsTrue(f.Remove(new TownId(0b_1110_1101)));
                Assert.IsTrue(f.Remove(new TownId(0b_1111_0000)));
                Assert.IsTrue(f.Remove(new TownId(0b_0011_1100)));

                Assert.IsNull(f.Find(new TownId(0)));
                Assert.IsNull(f.Find(new TownId(38)));
                Assert.IsNull(f.Find(new TownId(169)));
                Assert.IsNull(f.Find(new TownId(221)));
                Assert.IsNull(f.Find(new TownId(0b_1010_0101)));
                Assert.IsNull(f.Find(new TownId(0b_0110_1101)));
                Assert.IsNull(f.Find(new TownId(0b_0000_0101)));
                Assert.IsNull(f.Find(new TownId(0b_1000_0000)));
                Assert.IsNull(f.Find(new TownId(0b_1010_0110)));
                Assert.IsNull(f.Find(new TownId(0b_0011_0110)));
                Assert.IsNull(f.Find(new TownId(0b_1001_0111)));
                Assert.IsNull(f.Find(new TownId(0b_0000_1111)));
                Assert.IsNull(f.Find(new TownId(0b_1110_1101)));
                Assert.IsNull(f.Find(new TownId(0b_1111_0000)));
                Assert.IsNull(f.Find(new TownId(0b_0011_1100)));

                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(new string[0], actual);
            }
        }

        [TestMethod]
        public void AddAndFind_FirstItem_SholudAddToOverfillingFile()
        {
            using (var f = GetExtendibleHashingFileWith3BitDepthFilled())
            {
                Town town = new Town(0b_1111_0101, "Mesto 1");
                f.Add(town);
                Assert.AreEqual(town, f.Find(new TownId(0b_1111_0101)));

                f.Add(new Town(0, "Žilina"));

                var expected = new[] {
                    "Žilina",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Mesto 1" // In overfilling file
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void AddAndFind_SecondItemForTheSameBlock_SholudAddToTheSameOverfillingBlock()
        {
            using (var f = GetExtendibleHashingFileWith3BitDepthFilled())
            {
                Town town = new Town(0b_1111_0101, "Mesto 1");
                f.Add(town);
                Assert.AreEqual(town, f.Find(new TownId(0b_1111_0101)));

                town = new Town(0b_0111_0101, "Mesto 2");
                f.Add(town);
                Assert.AreEqual(town, f.Find(new TownId(0b_0111_0101)));

                f.Add(new Town(0, "Žilina"));

                var expected = new[] {
                    "Žilina",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Mesto 1", "Mesto 2" // In overfilling file
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void AddAndFind_MoreItemForTheSameBlock_SholudAddToExtendedOverfillingBlock()
        {
            using (var f = GetExtendibleHashingFileWith3BitDepthFilled())
            {
                f.Add(new Town(0b_1111_0101, "Mesto 1"));
                f.Add(new Town(0b_0111_0101, "Mesto 2"));
                f.Add(new Town(0b_1011_0101, "Mesto 3"));
                f.Add(new Town(0b_0011_0101, "Mesto 4"));
                f.Add(new Town(0b_1001_0101, "Mesto 5"));

                Town town = new Town(0b_1101_0101, "Mesto 6");
                f.Add(town);
                Assert.AreEqual(town, f.Find(new TownId(0b_1101_0101)));

                f.Add(new Town(0, "Žilina"));

                var expected = new[] {
                    "Žilina",
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Mesto 1", "Mesto 2","Mesto 3", "Mesto 4","Mesto 5", // In overfilling file
                    "Mesto 6" // In overfilling file
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_ItemsFromOverfillingBlock_SholudRemoveFromOverfillingBlockAndShrinkIt()
        {
            using (var f = GetExtendibleHashingFileWith3BitDepthFilled())
            {
                FileStream file = new FileStream(OverfillingFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                f.Add(new Town(0b_1111_0101, "Mesto 1"));
                f.Add(new Town(0b_0111_0101, "Mesto 2"));
                f.Add(new Town(0b_1011_0101, "Mesto 3"));
                f.Add(new Town(0b_0011_0101, "Mesto 4"));
                f.Add(new Town(0b_1001_0101, "Mesto 5"));
                Assert.AreEqual(OverfillingBlockByteSize, file.Length);

                f.Add(new Town(0b_1101_0101, "Mesto 6"));
                Assert.AreEqual(OverfillingBlockByteSize * 2, file.Length);

                Assert.IsTrue(f.Remove(new TownId(0b_1101_0101)));
                Assert.AreEqual(OverfillingBlockByteSize, file.Length);

                Assert.IsTrue(f.Remove(new TownId(0b_1111_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_0111_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_1011_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_0011_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_1001_0101)));
                Assert.AreEqual(0, file.Length);

                var expected = new[] {
                    "Levice", "Trnava", "Snina", "Senica", "Púchov"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_ItemFromBeginningOfOverfillingFile_SholudRemoveShrink()
        {
            using (var f = GetExtendibleHashingFileWith3BitDepthFilled())
            {
                FileStream file = new FileStream(OverfillingFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                f.Add(new Town(0b_1111_0101, "Mesto 1"));
                f.Add(new Town(0b_0111_0101, "Mesto 2"));
                f.Add(new Town(0b_1011_0101, "Mesto 3"));
                f.Add(new Town(0b_0011_0101, "Mesto 4"));
                f.Add(new Town(0b_1001_0101, "Mesto 5"));
                f.Add(new Town(0b_1101_0101, "Mesto 6"));
                Assert.AreEqual(OverfillingBlockByteSize * 2, file.Length);

                Assert.IsTrue(f.Remove(new TownId(0b_0111_0101))); // Mesto 2
                Assert.AreEqual(OverfillingBlockByteSize, file.Length);

                var expected = new[] {
                    "Levice", "Trnava", "Snina", "Senica", "Púchov",
                    "Mesto 1", "Mesto 3", "Mesto 4", "Mesto 5", "Mesto 6"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_ItemsFromMainFile_SholudRemoveAndAddToMainFileFromOverfillingFileAndShrink()
        {
            using (var f = GetExtendibleHashingFileWith3BitDepthFilled())
            {
                FileStream file = new FileStream(OverfillingFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                f.Add(new Town(0b_1111_0101, "Mesto 1"));
                f.Add(new Town(0b_0111_0101, "Mesto 2"));
                f.Add(new Town(0b_1011_0101, "Mesto 3"));
                f.Add(new Town(0b_0011_0101, "Mesto 4"));
                f.Add(new Town(0b_1001_0101, "Mesto 5"));
                f.Add(new Town(0b_1101_0101, "Mesto 6"));
                f.Add(new Town(0b_1100_0101, "Mesto 7"));
                Assert.AreEqual(OverfillingBlockByteSize * 2, file.Length);

                Assert.IsTrue(f.Remove(new TownId(0b_0110_1101))); // "Snina"
                Assert.AreEqual(OverfillingBlockByteSize * 2, file.Length);
                var expected = new[] {
                    "Levice", "Trnava", "Senica", "Púchov",
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5", "Mesto 6", "Mesto 7"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                Assert.IsTrue(f.Remove(new TownId(0b_1010_0101))); // "Trnava"
                Assert.AreEqual(OverfillingBlockByteSize, file.Length);
                expected = new[] {
                    "Levice", "Senica", "Púchov", "Mesto 7", "Mesto 6",
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5"
                };
                actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_AllItemsFromMainFile_SholudTransferItemsToMaiFileOnlyIfItIsEmpty()
        {
            using (var f = GetExtendibleHashingFileWith3BitDepthFilled())
            {
                FileStream file = new FileStream(OverfillingFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                f.Add(new Town(0b_1111_0101, "Mesto 1"));
                f.Add(new Town(0b_0111_0101, "Mesto 2"));
                f.Add(new Town(0b_1011_0101, "Mesto 3"));
                f.Add(new Town(0b_0011_0101, "Mesto 4"));
                f.Add(new Town(0b_1001_0101, "Mesto 5"));
                Assert.AreEqual(OverfillingBlockByteSize, file.Length);

                var expected = new[] {
                    "Levice", "Trnava","Snina", "Senica", "Púchov",
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                Assert.IsTrue(f.Remove(new TownId(0b_1101_1101))); // "Levice"
                Assert.IsTrue(f.Remove(new TownId(0b_1010_0101))); // "Trnava"
                Assert.IsTrue(f.Remove(new TownId(0b_0110_1101))); // "Snina"
                Assert.IsTrue(f.Remove(new TownId(0b_0000_0101))); // "Senica"

                expected = new[] {
                    "Púchov",
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5"
                };
                actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                Assert.IsTrue(f.Remove(new TownId(0b_1110_1101))); // "Púchov"

                Assert.AreEqual(0, file.Length);
                expected = new[] {
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5"
                };
                actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Remove_AllItemsFromMainFileMoreItemInOverfillFile_SholudTransferItemsToMaiFileOnlyIfItIsEmpty()
        {
            using (var f = GetExtendibleHashingFileWith3BitDepthFilled())
            {
                FileStream file = new FileStream(OverfillingFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                f.Add(new Town(0b_1111_0101, "Mesto 1"));
                f.Add(new Town(0b_0111_0101, "Mesto 2"));
                f.Add(new Town(0b_1011_0101, "Mesto 3"));
                f.Add(new Town(0b_0011_0101, "Mesto 4"));
                f.Add(new Town(0b_1001_0101, "Mesto 5"));
                f.Add(new Town(0b_0101_0101, "Mesto 6"));
                f.Add(new Town(0b_1100_0101, "Mesto 7"));
                f.Add(new Town(0b_1000_0101, "Mesto 8"));
                f.Add(new Town(0b_0100_0101, "Mesto 9"));
                f.Add(new Town(0b_0010_0101, "Mesto 10"));
                Assert.AreEqual(OverfillingBlockByteSize * 2, file.Length);

                var expected = new[] {
                    "Levice", "Trnava","Snina", "Senica", "Púchov",
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5",
                    "Mesto 6", "Mesto 7", "Mesto 8", "Mesto 9", "Mesto 10"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                Assert.IsTrue(f.Remove(new TownId(0b_1101_1101))); // "Levice"
                Assert.IsTrue(f.Remove(new TownId(0b_1010_0101))); // "Trnava"
                Assert.IsTrue(f.Remove(new TownId(0b_0110_1101))); // "Snina"
                Assert.IsTrue(f.Remove(new TownId(0b_0000_0101))); // "Senica"

                expected = new[] {
                    "Púchov",
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5",
                    "Mesto 6", "Mesto 7", "Mesto 8", "Mesto 9", "Mesto 10"
                };
                actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
                Assert.AreEqual(OverfillingBlockByteSize * 2, file.Length);

                Assert.IsTrue(f.Remove(new TownId(0b_1110_1101))); // "Púchov"

                Assert.AreEqual(OverfillingBlockByteSize, file.Length);
                expected = new[] {
                    "Mesto 10", "Mesto 9", "Mesto 8", "Mesto 7", "Mesto 6",
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5"
                };
                actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
                Assert.AreEqual(OverfillingBlockByteSize, file.Length);
            }
        }

        [TestMethod]
        public void Remove_EmptyOverfillingFile_SholudNotDoAnything()
        {
            using (var f = GetExtendibleHashingFileWith3BitDepthFilled())
            {
                Assert.IsFalse(f.Remove(new TownId(999999)));
                Assert.IsFalse(f.Remove(new TownId(888888)));
                Assert.IsFalse(f.Remove(new TownId(777777)));

                f.Add(new Town(0b_1111_0101, "Mesto 1"));

                var expected = new[] {
                    "Levice", "Trnava","Snina", "Senica", "Púchov",
                    "Mesto 1"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                Assert.IsFalse(f.Remove(new TownId(999999)));

                Assert.IsTrue(f.Remove(new TownId(0b_1111_0101))); //  "Mesto 1"

                expected = new[] {
                    "Levice", "Trnava","Snina", "Senica", "Púchov"
                };
                actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                Assert.IsFalse(f.Remove(new TownId(888888)));
                Assert.IsFalse(f.Remove(new TownId(777777)));

                f.Add(new Town(0b_0111_0101, "Mesto 2"));

                expected = new[] {
                    "Levice", "Trnava","Snina", "Senica", "Púchov",
                    "Mesto 2"
                };
                actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void LoadingFilesFromOverfillingFile_ValidFiles_ShouldLoadAllDataFromFiles()
        {
            var f = GetExtendibleHashingFileWith3BitDepthFilled();
            f.Add(new Town(0b_1111_0101, "Mesto 1"));
            f.Add(new Town(0b_0111_0101, "Mesto 2"));
            f.Add(new Town(0b_1011_0101, "Mesto 3"));
            f.Add(new Town(0b_0011_0101, "Mesto 4"));
            f.Add(new Town(0b_1001_0101, "Mesto 5"));
            f.Add(new Town(0b_0101_0101, "Mesto 6"));
            f.Add(new Town(0b_1100_0101, "Mesto 7"));
            f.Add(new Town(0b_1000_0101, "Mesto 8"));
            f.Add(new Town(0b_0100_0101, "Mesto 9"));
            f.Add(new Town(0b_0010_0101, "Mesto 10"));
            f.Dispose();

            using (f = new ExtendibleHashingFile<Town>(FilePath, OverfillingFilePath, ManagerFilePath, OverfillingManagerFilePath))
            {
                var expected = new[] {
                    "Levice", "Trnava","Snina", "Senica", "Púchov",
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5",
                    "Mesto 6", "Mesto 7", "Mesto 8", "Mesto 9", "Mesto 10"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                f.Add(new Town(0, "Žilina"));
                f.Add(new Town(0b_0001_0101, "Mesto 11"));

                expected = new[] {
                    "Žilina",
                    "Levice", "Trnava","Snina", "Senica", "Púchov",
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5",
                    "Mesto 6", "Mesto 7", "Mesto 8", "Mesto 9", "Mesto 10",
                    "Mesto 11"
                };
                actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void LoadingFilesFromOverfillingFile_NoData_ShouldLoadNoDataCorrectly()
        {
            var f = GetExtendibleHashingFileWith3BitDepth();
            f.Dispose();

            using (f = new ExtendibleHashingFile<Town>(FilePath, OverfillingFilePath, ManagerFilePath, OverfillingManagerFilePath))
            {
                var expected = new string[0];
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                f.Add(new Town(0b_1111_0101, "Mesto 1"));
                f.Add(new Town(0b_0111_0101, "Mesto 2"));
                f.Add(new Town(0b_1011_0101, "Mesto 3"));
                f.Add(new Town(0b_0011_0101, "Mesto 4"));
                f.Add(new Town(0b_1001_0101, "Mesto 5"));
                f.Add(new Town(0b_0101_0101, "Mesto 6"));
                f.Add(new Town(0b_1100_0101, "Mesto 7"));
                f.Add(new Town(0b_1000_0101, "Mesto 8"));
                f.Add(new Town(0b_0100_0101, "Mesto 9"));
                f.Add(new Town(0b_0010_0101, "Mesto 10"));
            }

            using (f = new ExtendibleHashingFile<Town>(FilePath, OverfillingFilePath, ManagerFilePath, OverfillingManagerFilePath))
            {
                var expected = new[] {
                    "Mesto 1", "Mesto 2", "Mesto 3", "Mesto 4", "Mesto 5",
                    "Mesto 6", "Mesto 7", "Mesto 8", "Mesto 9", "Mesto 10"
                };
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);

                Assert.IsTrue(f.Remove(new TownId(0b_1111_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_0111_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_1011_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_0011_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_1001_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_0101_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_1100_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_1000_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_0100_0101)));
                Assert.IsTrue(f.Remove(new TownId(0b_0010_0101)));
            }

            using (f = new ExtendibleHashingFile<Town>(FilePath, OverfillingFilePath, ManagerFilePath, OverfillingManagerFilePath))
            {
                var expected = new string[0];
                var actual = f.Select(t => t.Name).ToArray();
                CollectionAssert.AreEqual(expected, actual);
            }
        }

    }
}
