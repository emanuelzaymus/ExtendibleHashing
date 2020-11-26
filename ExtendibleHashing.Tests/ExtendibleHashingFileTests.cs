using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendibleHashing.Tests.TestClasses;
using System.Linq;
using System.IO;
using System.Text;

namespace ExtendibleHashing.Tests
{
    [TestClass]
    public class ExtendibleHashingFileTests
    {
        private const string FilePath = "test_extendibleHashingFile.bin";
        private const string OverfillingFilePath = "test_overfillingFile.bin";
        private const string ManagerFilePath = "test_managerFile.txt";
        private const int BlockByteSize = 256; // Town.ByteSize = 48

        private ExtendibleHashingFile<Town> GetExtendibleHashingFile()
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
            f.Add(new Town(0b_0011_0110, "Nitra"));
            f.Add(new Town(0b_0000_0000, "Poprad"));
            f.Add(new Town(0b_0010_0110, "Lučenec"));
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
            var f = GetExtendibleHashingFile();
            Town t = new Town(14, "Zilina");
            f.Add(t);
            CollectionAssert.AreEqual(new[] { t }, f.ToArray());
        }

        [TestMethod]
        public void Add_MoreValue_ShouldAddToRightBlocks()
        {
            var f = GetExtendibleHashingFileFilled();
            var expected = new[] { "Žilina", "Poprad", "Ilava", "Brezno", "Košice", "Lučenec", "Nitra",
                "Martin", "Púchov", "Levice", "Trnava", "Snina", "Senica", "Zvolen", "Prešov" };
            CollectionAssert.AreEqual(expected, f.Select(t => t.Name).ToArray());
        }

    }
}
