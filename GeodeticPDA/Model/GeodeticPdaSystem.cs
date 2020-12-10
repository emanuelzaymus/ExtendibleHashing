using ExtendibleHashing;
using GeodeticPDA.DataGeneration;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace GeodeticPDA.Model
{
    class GeodeticPdaSystem : IDisposable
    {
        private const string NextPropertyIdFilePath = "AppData-NextPropertyId.txt";

        private const string FilePath = "AppData-ExtendibleHashingFile.bin";
        private const string OverfillingFilePath = "AppData-OverfillingFile.bin";
        private const string ManagerFilePath = "AppData-ManagerFile.txt";
        private const string OverfillingManagerFilePath = "AppData-OverfillingManagerFile.txt";
        private const int BlockByteSize = 424; // (Property.ByteSize = 84) * 5 + (ValidItemsCount = 4) = 420 + (4) = 424
        private const int OverfillingBlockByteSize = 844; // 420 * 2 + (ValidItemsCount = 4) = 844

        private const int MaxBitDepth = 3;

        private ExtendibleHashingFile<Property> _file = new ExtendibleHashingFile<Property>(FilePath,
            OverfillingFilePath, ManagerFilePath, OverfillingManagerFilePath, BlockByteSize, OverfillingBlockByteSize, maxBitDepth: MaxBitDepth);

        public GeodeticPdaSystem()
        {
            if (File.Exists(NextPropertyIdFilePath))
            {
                string nextIdStr = File.ReadAllText(NextPropertyIdFilePath);
                if (int.TryParse(nextIdStr, out int nextId))
                {
                    Property.NextId = nextId;
                }
            }
        }

        /// <summary>
        /// Adds property.
        /// </summary>
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

        /// <summary>
        /// Find property by it's Id.
        /// </summary>
        internal Property FindProperty(int id)
        {
            return _file.Find(new PropertyId(id));
        }

        /// <summary>
        /// Removes property by it's Id if such a property exists.
        /// </summary>
        internal bool RemoveProperty(int id)
        {
            return _file.Remove(new PropertyId(id));
        }

        /// <summary>
        /// Updates changes for edited property.
        /// </summary>
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

        /// <summary>
        /// Generates random properties.
        /// </summary>
        internal void GenerateProperties(int count)
        {
            foreach (var item in RandomPropertyGenerator.GenerateProperties(count))
            {
                _file.Add(item);
            }
        }

        /// <summary>
        /// Gets all main file items.
        /// </summary>
        internal IEnumerable MainFileItems()
        {
            return _file.MainFileItems();
        }

        /// <summary>
        /// Gets all overfilling file items.
        /// </summary>
        internal IEnumerable OverfillingFileItems()
        {
            return _file.OverfillingFileItems();
        }

        /// <summary>
        /// Gets managing data.
        /// </summary>
        internal string GetManagingData()
        {
            return _file.GetManagingData();
        }

        public void Dispose()
        {
            _file.Dispose();
            File.WriteAllText(NextPropertyIdFilePath, Property.NextId.ToString());
        }

    }
}
