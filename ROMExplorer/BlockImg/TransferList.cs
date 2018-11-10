// 
//  ROMExplorer
// 
//  Copyright 2018 Martin Gerczuk
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software 
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ROMExplorer.BlockImg
{
    internal class TransferList
    {
        public const int BLOCKSIZE = 4096;

        public TransferList(Stream stream)
        {
            // see https://android.googlesource.com/platform/bootable/recovery/+/android-5.1.1_r1/updater/blockimg.c
            // and https://android.googlesource.com/platform/bootable/recovery/+/master/updater/blockimg.cpp

            using (var reader = new StreamReader(stream))
            {
                //var lines = reader.ReadToEnd().Split('\n');
                //int inx = 0;

                // First line in transfer list is the version number.
                Version = int.Parse(reader.ReadLine());

                // Second line in transfer list is the total number of blocks we expect to write.
                TotalBlocks = int.Parse(reader.ReadLine());

                if (Version >= 2)
                {
                    // Third line is how many stash entries are needed simultaneously.
                    StashEntries = int.Parse(reader.ReadLine());

                    // Fourth line is the maximum number of blocks that will be stashed simultaneously
                    StashMaxBlocks = int.Parse(reader.ReadLine());
                }

                var line = reader.ReadLine();
                while (line != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var tokens = line.Split(' ');

                        var entry = CreateEntry(tokens);
                        Entries.Add(entry);
                    }
                    line = reader.ReadLine();
                }
            }
        }

        public int Version { get; }

        public int TotalBlocks { get; }
        public int StashEntries { get; }
        public int StashMaxBlocks { get; }

        public IList<ITransferListEntry> Entries { get; } = new List<ITransferListEntry>();

        private static ITransferListEntry CreateEntry(string[] tokens)
        {
            switch (tokens[0])
            {
                case "erase":
                    return new EraseEntry(Range.Parse(tokens[1]));
                case "new":
                    return new NewEntry(Range.Parse(tokens[1]));
                case "zero":
                    return new ZeroEntry(Range.Parse(tokens[1]));
                default:
                    throw new ArgumentException();
            }
        }

        private class Range
        {
            private Range(int from, int to)
            {
                From = from;
                To = to;
            }

            public int From { get; }
            public int To { get; }

            public static IEnumerable<Range> Parse(string s)
            {
                var parts = s.Split(',');

                var num = int.Parse(parts[0]);
                if (num != parts.Length - 1)
                    throw new ArgumentException();

                for (var i = 0; i < num; i += 2)
                {
                    var from = int.Parse(parts[i + 1]);
                    var to = int.Parse(parts[i + 2]);

                    yield return new Range(from, to);
                }
            }
        }

        private class EraseEntry : ITransferListEntry
        {
            private readonly List<Range> rangeSet;

            public EraseEntry(IEnumerable<Range> rangeSet)
            {
                this.rangeSet = rangeSet.ToList();
            }

            #region Implementation of ITransferListEntry

            public void Perform(Stream inStream, Stream outStream)
            {
            }

            #endregion
        }

        private class NewEntry : ITransferListEntry
        {
            private readonly List<Range> rangeSet;

            public NewEntry(IEnumerable<Range> rangeSet)
            {
                this.rangeSet = rangeSet.ToList();
            }

            #region Implementation of ITransferListEntry

            public void Perform(Stream inStream, Stream outStream)
            {
                foreach (var range in rangeSet)
                {
                    outStream.Position = range.From * BLOCKSIZE;
                    var buf = new byte[BLOCKSIZE];
                    var nBlocks = range.To - range.From;
                    for (var i = 0; i < nBlocks; i++)
                    {
                        inStream.Read(buf, 0, BLOCKSIZE);
                        outStream.Write(buf, 0, BLOCKSIZE);
                    }
                }
            }

            #endregion
        }

        private class ZeroEntry : ITransferListEntry
        {
            private readonly List<Range> rangeSet;

            public ZeroEntry(IEnumerable<Range> rangeSet)
            {
                this.rangeSet = rangeSet.ToList();
            }

            #region Implementation of ITransferListEntry

            public void Perform(Stream inStream, Stream outStream)
            {
                foreach (var range in rangeSet)
                {
                    outStream.Position = range.From * BLOCKSIZE;
                    var buf = new byte[BLOCKSIZE];
                    var nBlocks = range.To - range.From;
                    for (var i = 0; i < nBlocks; i++)
                        outStream.Write(buf, 0, BLOCKSIZE);
                }
            }

            #endregion
        }
    }
}