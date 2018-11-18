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
using ROMExplorer.Utils;

namespace ROMExplorer.Sin
{
    internal class SinParserV3
    {
        public static byte[] MMCF = {(byte) 'M', (byte) 'M', (byte) 'C', (byte) 'F'};
        public static byte[] ADDR = {(byte) 'A', (byte) 'D', (byte) 'D', (byte) 'R'};
        public static byte[] LZ4A = {(byte) 'L', (byte) 'Z', (byte) '4', (byte) 'A'};

        private readonly Stream stream;
        private readonly Header header;
        private readonly IList<HashBlock> blocks = new List<HashBlock>();
        private readonly byte[] cert;
        private readonly int certLen;
        private readonly List<object> dataBlocks;
        private readonly DataHeader dataHeader;

        public SinParserV3(Stream stream)
        {
            this.stream = stream;
            var r = new BinaryReader(stream);
            header = Header.Read(r);

            if (header.hashLen > header.headerLen) throw new Exception("Error parsing sin file");

            var pos = stream.Position;

            while (stream.Position < pos + header.hashLen)
                blocks.Add(HashBlock.Read(r, header.hashType));

            certLen = r.ReadInt32BE();
            cert = r.ReadBytes(certLen);

            var mmcfMagic = r.ReadBytes(4);
            if (mmcfMagic.SequenceEqual(MMCF))
            {
                dataHeader = DataHeader.Read(r);
                var addrLength = dataHeader.mmcfLen - dataHeader.gptpLen;
                long read = 0;
                dataBlocks = new List<object>();
                while (read < addrLength)
                {
                    var amagic = r.ReadBytes(4);
                    read += 4;

                    if (amagic.SequenceEqual(ADDR))
                    {
                        var addrBlock = AddrBlock.Read(r);
                        dataBlocks.Add(addrBlock);
                        read += addrBlock.blockLen - 4;
                    }
                    else if (amagic.SequenceEqual(LZ4A))
                    {
                        // TODO: not yet tested...
                        var lz4Block = LZ4Block.Read(r);
                        dataBlocks.Add(lz4Block);
                        read += lz4Block.blockLen - 4;
                    }
                    else
                    {
                        throw new Exception("unexpected magic pattern.");
                    }
                }
            }
            else
            {
                dataHeader = new DataHeader {gptpLen = 0, mmcfLen = 0};
            }
        }

        public void CopyTo(Stream outStream)
        {
            if (dataHeader.mmcfLen > 0)
            {
                var dataOffset = header.headerLen + dataHeader.mmcfLen + 8;

                foreach (var block in dataBlocks)
                {
                    if (block is AddrBlock addrBlock)
                    {
                        stream.Seek(dataOffset + addrBlock.dataOffset, SeekOrigin.Begin);
                        outStream.Seek(addrBlock.fileOffset, SeekOrigin.Begin);
                        var data = new byte[addrBlock.dataLen];
                        var nbread = stream.Read(data, 0, (int) addrBlock.dataLen);
                        outStream.Write(data, 0, (int)addrBlock.dataLen);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private struct Header
        {
            public byte[] magic;
            public int headerLen;
            public int payloadType;
            public int hashType;
            public int reserved;
            public int hashLen;

            public static Header Read(BinaryReader r)
            {
                var h = new Header();

                h.magic = r.ReadBytes(3);
                h.headerLen = r.ReadInt32BE();
                h.payloadType = r.ReadInt32BE();
                h.hashType = r.ReadInt32BE();
                h.reserved = r.ReadInt32BE();
                h.hashLen = r.ReadInt32BE();

                return h;
            }
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

        private struct DataHeader
        {
            //public byte[] mmcfMagic;
            public int mmcfLen;

            public byte[] gptpMagic;
            public int gptpLen;
            public byte[] gptpuid;

            public static DataHeader
                Read(BinaryReader r)
            {
                var h = new DataHeader();

                //h.mmcfMagic = r.ReadBytes(4);

                //if (!h.mmcfMagic.SequenceEqual(MMCF))
                //    return null;

                h.mmcfLen = r.ReadInt32BE();
                h.gptpMagic = r.ReadBytes(4);
                h.gptpLen = r.ReadInt32BE();
                h.gptpuid = r.ReadBytes(h.gptpLen - 8);

                return h;
            }
        }

        private struct AddrBlock
        {
            public int blockLen;
            public long dataOffset;
            public long dataLen;
            public long fileOffset;
            public int hashType;
            public byte[] checksum;

            public static AddrBlock Read(BinaryReader r)
            {
                var b = new AddrBlock();

                b.blockLen = r.ReadInt32BE();
                b.dataOffset = r.ReadInt64BE();
                b.dataLen = r.ReadInt64BE();
                b.fileOffset = r.ReadInt64BE();
                b.hashType = r.ReadInt32BE();
                b.checksum = r.ReadBytes(b.blockLen - 36);

                return b;
            }
        }

        private struct LZ4Block
        {
            public int blockLen;
            public long dataOffset;
            public long uncompDataLen;
            public long compDataLen;
            public long fileOffset;
            public long reserved;
            public int hashType;
            public byte[] checksum;

            public static LZ4Block Read(BinaryReader r)
            {
                var b = new LZ4Block();

                b.blockLen = r.ReadInt32BE();
                b.dataOffset = r.ReadInt64BE();
                b.uncompDataLen = r.ReadInt64BE();
                b.compDataLen = r.ReadInt64BE();
                b.fileOffset = r.ReadInt64BE();
                b.reserved = r.ReadInt64BE();
                b.hashType = r.ReadInt32BE();
                b.checksum = r.ReadBytes(b.blockLen - 52);

                return b;
            }
        }
    }
}