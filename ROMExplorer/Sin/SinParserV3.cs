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
using K4os.Compression.LZ4;
using ROMExplorer.Utils;

namespace ROMExplorer.Sin
{
    // This class is inspired by Androxyde's Flashtool
    // https://github.com/Androxyde/Flashtool
    internal class SinParserV3
    {
        public static void CopyTo(BinaryReader reader, Stream outStream)
        {
            // This implementation uses a BinaryReader without any Seek operation
            // so it can be used with an archive stream

            var header = SinHeader.Read(reader);
            var dataHeader = MmcfBlock.Read(reader);

            var orderedBlocks = dataHeader.DataBlocks.OrderBy(b => b.DataOffset).ToList();

            var dummy = reader.ReadInt64BE(); // ??
            var pos = 0L;

            var firstOffset = orderedBlocks.FirstOrDefault()?.DataOffset ?? 0L;
            if (firstOffset > pos)
            {
                reader.ReadBytes((int) firstOffset); // ??
                pos = firstOffset;
            }

            var totalSize = orderedBlocks.Max(b => b.FileOffset + b.FileLen);

            foreach (var block in orderedBlocks)
            {
                if (pos != block.DataOffset) throw new Exception("Unexpected");

                var data = reader.ReadBytes((int) block.DataLen);
                pos += block.DataLen;

                if (block is AddrBlock)
                {
                    // fall through
                }
                else if (block is LZ4Block lz4Block)
                {
                    var uncompData = new byte[lz4Block.UncompDataLen];
                    LZ4Codec.Decode(data, uncompData);
                    data = uncompData;
                }
                else
                {
                    throw new NotImplementedException();
                }

                FileInfoBase.ReportProgress(outStream.Length, totalSize);

                outStream.Seek(block.FileOffset, SeekOrigin.Begin);
                outStream.Write(data, 0, data.Length);
            }
        }

        // SIN file header seems to have some kind of tag-length-value structure

        private abstract class BlockBase
        {
            public int Tag { get; private set; }
            public int Length { get; private set; }

            protected void Init(BinaryReader r, int tag)
            {
                Tag = tag;
                Length = r.ReadInt32BE();
                var reader = new BinaryReader(new MemoryStream(r.ReadBytes(Length - 8)));
                ReadData(reader);
            }

            protected abstract void ReadData(BinaryReader r);
        }

        private class SinHeader : BlockBase
        {
            private const int TAG = 0x0353494e; // 0x03 "SIN"

            public IList<HashBlock> Blocks { get; } = new List<HashBlock>();

            public static SinHeader Read(BinaryReader r)
            {
                var h = new SinHeader();
                h.Init(r, r.ReadInt32BE());
                return h;
            }

            #region Overrides of BlockBase

            protected override void ReadData(BinaryReader r)
            {
                if (Tag != TAG) throw new Exception($"Invalid header magic number 0x{Tag:4X}");

                var payloadType = r.ReadInt32BE();
                var hashType = r.ReadInt32BE();
                var reserved = r.ReadInt32BE();
                var hashLen = r.ReadInt32BE();

                if (hashLen > Length) throw new Exception("Error parsing sin file");

                var pos = r.BaseStream.Position;

                while (r.BaseStream.Position < pos + hashLen)
                    Blocks.Add(HashBlock.Read(r, hashType));

                var certLen = r.ReadInt32BE();
                certLen = (certLen + 3) / 4 * 4; // ???
                var cert = r.ReadBytes(certLen);
            }

            #endregion
        }

        private struct HashBlock
        {
            private static readonly byte[] hashv3len = {0, 0, 32};

            public int length;
            public byte[] crc;

            public static HashBlock Read(BinaryReader r, int hashType)
            {
                var b = new HashBlock();

                b.length = r.ReadInt32BE();
                b.crc = r.ReadBytes(hashv3len[hashType]);

                return b;
            }
        }

        private class MmcfBlock : BlockBase
        {
            private const int TAG = 0x4d4d4346; // "MMCF"

            public List<CopyBlockBase> DataBlocks { get; } = new List<CopyBlockBase>();

            public static MmcfBlock Read(BinaryReader r)
            {
                var h = new MmcfBlock();
                h.Init(r, r.ReadInt32BE());
                return h;
            }

            #region Overrides of BlockBase

            protected override void ReadData(BinaryReader r)
            {
                if (Tag != TAG) throw new Exception($"Invalid header magic number 0x{Tag:4X}");

                GptpData.Read(r);

                while (r.PeekChar() >= 0)
                {
                    var tag = r.ReadInt32BE();
                    switch (tag)
                    {
                        case AddrBlock.TAG: // ADDR
                            DataBlocks.Add(AddrBlock.Read(r, tag));
                            break;
                        case LZ4Block.TAG: // LZ4A
                            DataBlocks.Add(LZ4Block.Read(r, tag));
                            break;
                        default:
                            throw new Exception("Unknown block magic number 0x{tag:4X}");
                    }
                }
            }

            #endregion
        }

        private class GptpData : BlockBase
        {
            private const int TAG = 0x47505450; // "GPTP"

            public static GptpData Read(BinaryReader r)
            {
                var h = new GptpData();
                h.Init(r, r.ReadInt32BE());
                return h;
            }

            #region Overrides of BlockBase

            protected override void ReadData(BinaryReader r)
            {
                if (Tag != TAG) throw new Exception($"Invalid header magic number 0x{Tag:4X}");

                var gptpuid = r.ReadBytes(Length - 8);
            }

            #endregion
        }

        private abstract class CopyBlockBase : BlockBase
        {
            public long DataLen { get; protected set; }
            public long DataOffset { get; protected set; }
            public long FileOffset { get; protected set; }
            public abstract long FileLen { get; }
            public int HashType { get; protected set; }
            public byte[] Checksum { get; protected set; }
        }

        private class AddrBlock : CopyBlockBase
        {
            public const int TAG = 0x41444452; // "ADDR"

            #region Overrides of CopyBlockBase

            public override long FileLen => DataLen;

            #endregion

            public static AddrBlock Read(BinaryReader r, int tag)
            {
                var h = new AddrBlock();
                h.Init(r, tag);
                return h;
            }

            #region Overrides of BlockBase

            protected override void ReadData(BinaryReader r)
            {
                DataOffset = r.ReadInt64BE();
                DataLen = r.ReadInt64BE();
                FileOffset = r.ReadInt64BE();
                HashType = r.ReadInt32BE();
                Checksum = r.ReadBytes(Length - 36);
            }

            #endregion
        }

        private class LZ4Block : CopyBlockBase
        {
            public const int TAG = 0x4C5A3441; // "LZ4A"

            public long UncompDataLen { get; private set; }

            #region Overrides of CopyBlockBase

            public override long FileLen => UncompDataLen;

            #endregion

            public static LZ4Block Read(BinaryReader r, int tag)
            {
                var h = new LZ4Block();
                h.Init(r, tag);
                return h;
            }

            #region Overrides of BlockBase

            protected override void ReadData(BinaryReader r)
            {
                DataOffset = r.ReadInt64BE();
                UncompDataLen = r.ReadInt64BE();
                DataLen = r.ReadInt64BE();
                FileOffset = r.ReadInt64BE();
                var reserved = r.ReadInt64BE();
                HashType = r.ReadInt32BE();
                Checksum = r.ReadBytes(Length - 52);
            }

            #endregion
        }
    }
}