using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendibleHashing.Tests.TestClasses;
using System.Linq;
using System.IO;

namespace ExtendibleHashing.Tests
{
    [TestClass]
    public class ExtendibleHashingFileTests
    {
        private const string FilePath = "test_extendibleHashingFile.bin";
        private const string OverfillingFilePath = "test_overfillingFile.bin";
        private const string ManagerFilePath = "test_managerFile.txt";
        private const int BlockByteSize = 256; // (Town.ByteSize = 48) * 5

        protected ExtendibleHashingFile<Town> GetExtendibleHashingFile()
        {
            return new ExtendibleHashingFile<Town>(FilePath, OverfillingFilePath, ManagerFilePath, BlockByteSize, FileMode.Create);
        }

        private ExtendibleHashingFile<Town> GetExtendibleHashingFileFilled()
        {
            var f = GetExtendibleHashingFile();
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

    }
}
