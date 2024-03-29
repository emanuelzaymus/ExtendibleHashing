﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExtendibleHashing.FileHandlers
{
    class ManagingFileHandler : TextFileHandler
    {
        public ManagingFileHandler(string path) : base(path) { }

        internal bool Read(out int blockByteSize, out List<int> blockAddresses, out List<int> blockBitDepths,
            out List<int> blockItemCounts, out List<bool> fileBlockOccupation, out int fBitDepth, out int mBitDepth)
        {
            blockByteSize = -1;
            blockAddresses = null;
            blockBitDepths = null;
            blockItemCounts = null;
            fileBlockOccupation = null;
            fBitDepth = -1;
            mBitDepth = -1;

            try
            {
                string[] lines = File.ReadAllLines(_path);
                blockByteSize = int.Parse(lines[0]);
                blockAddresses = lines[1].Split(Separator).Select(x => int.Parse(x)).ToList();
                blockBitDepths = lines[2].Split(Separator).Select(x => int.Parse(x)).ToList();
                blockItemCounts = lines[3].Split(Separator).Select(x => int.Parse(x)).ToList();
                fileBlockOccupation = lines[4].Split(Separator).Select(x => bool.Parse(x)).ToList();
                fBitDepth = int.Parse(lines[5]);
                mBitDepth = int.Parse(lines[6]);

                return blockByteSize >= 1
                    && blockAddresses.Count == blockBitDepths.Count
                    && blockAddresses.Count == blockItemCounts.Count
                    && fBitDepth == Math.Log(blockAddresses.Count, 2)
                    && fBitDepth <= mBitDepth;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal void Write(int blockByteSize, List<int> blockAddresses, List<int> blockBitDepths,
            List<int> blockItemCounts, List<bool> fileBlockOccupation, int fileBitDepth, int maxBitDepth)
        {
            var linesToWrite = new string[7];
            linesToWrite[0] = blockByteSize.ToString();
            linesToWrite[1] = string.Join(Separator.ToString(), blockAddresses);
            linesToWrite[2] = string.Join(Separator.ToString(), blockBitDepths);
            linesToWrite[3] = string.Join(Separator.ToString(), blockItemCounts);
            linesToWrite[4] = string.Join(Separator.ToString(), fileBlockOccupation);
            linesToWrite[5] = fileBitDepth.ToString();
            linesToWrite[6] = maxBitDepth.ToString();

            File.WriteAllLines(_path, linesToWrite);
        }

    }
}
