using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExtendibleHashing.FileHandlers
{
    class OverfillingManagingFileHandler : TextFileHandler
    {
        public OverfillingManagingFileHandler(string path) : base(path) { }

        internal bool Read(out int blockByteSize, out List<bool> fileBlockOccupation, out List<List<OverfillingBlockInfo>> blocksInfo)
        {
            blockByteSize = -1;
            fileBlockOccupation = null;
            blocksInfo = null;

            try
            {
                string[] lines = File.ReadAllLines(_path);
                blockByteSize = int.Parse(lines[0]);
                fileBlockOccupation = lines[1].Split(Separator).Select(x => bool.Parse(x)).ToList();
                int maxItemCount = int.Parse(lines[2]);

                blocksInfo = new List<List<OverfillingBlockInfo>>();

                int lastMainFileAddress = -1;
                for (int i = 3; i < lines.Length; i++)
                {
                    var numbers = lines[i].Split(Separator).Select(x => int.Parse(x)).ToArray();
                    int mainFileAddress = numbers[0];
                    int address = numbers[1];
                    int itemCount = numbers[2];

                    if (lastMainFileAddress != mainFileAddress)
                    {
                        lastMainFileAddress = mainFileAddress;
                        blocksInfo.Add(new List<OverfillingBlockInfo>());
                    }
                    blocksInfo[blocksInfo.Count - 1].Add(new OverfillingBlockInfo(mainFileAddress, address, maxItemCount, itemCount));
                }

                return blockByteSize > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal void Write(int blockByteSize, List<bool> blockOccupation, List<List<OverfillingBlockInfo>> blocksInfo)
        {
            var linesToWrite = new List<string>();
            linesToWrite.Add(blockByteSize.ToString()); // 0
            linesToWrite.Add(string.Join(Separator.ToString(), blockOccupation)); // 1

            bool first = true;

            foreach (var infoList in blocksInfo)
            {
                foreach (var info in infoList)
                {
                    if (first)
                    {
                        first = false;
                        linesToWrite.Add(info.MaxItemCount.ToString()); // 2
                    }
                    linesToWrite.Add(string.Join(Separator.ToString(), info.MainFileAddress, info.Address, info.ItemCount)); // 3 .. n
                }
            }
            File.WriteAllLines(_path, linesToWrite);
        }

    }
}
