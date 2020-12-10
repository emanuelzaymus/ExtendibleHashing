using ExtendibleHashing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GeodeticPDA.Model
{
    class GeodeticPdaSystem : IDisposable
    {
        private const string FilePath = "extendibleHashingFile.bin";
        private const string OverfillingFilePath = "overfillingFile.bin";
        private const string ManagerFilePath = "managerFile.txt";
        private const string OverfillingManagerFilePath = "overfillingManagerFile.txt";
        private const int BlockByteSize = 424; // (Property.ByteSize = 84) * 5 + (ValidItemsCount = 4) = 420 + 4 = 424
        private const int OverfillingBlockByteSize = 844; // 420 * 2 + 4 = 844

        private ExtendibleHashingFile<Property> _file = new ExtendibleHashingFile<Property>(FilePath,
            OverfillingFilePath, ManagerFilePath, OverfillingManagerFilePath, BlockByteSize, OverfillingBlockByteSize);

        internal bool AddProperty(Property newProperty)
        {
            try
            {
                _file.Add(newProperty);
                return true;
            }
            catch (ArgumentException e)
            {
                Trace.WriteLine(e.Message);
                return false;
            }
        }

        internal Property FindProperty(int id)
        {
            return _file.Find(new PropertyId(id));
        }

        internal bool RemoveProperty(int id)
        {
            return _file.Remove(new PropertyId(id));
        }

        internal bool UpdateProperty(int oldId, Property newProperty)
        {
            if (oldId == newProperty.Id)
            {
                return _file.Update(new PropertyId(oldId), newProperty);
            }
            else if (RemoveProperty(oldId))
            {
                AddProperty(newProperty);
                return true;
            }
            Trace.WriteLine($"Old id {oldId} does not exist in the file.");
            return false;
        }

        internal IEnumerable MainFileItems()
        {
            return _file.MainFileItems();
        }

        internal IEnumerable OverfillingFileItems()
        {
            return _file.OverfillingFileItems();
        }

        public void Dispose()
        {
            _file.Dispose();
        }

    }
}
