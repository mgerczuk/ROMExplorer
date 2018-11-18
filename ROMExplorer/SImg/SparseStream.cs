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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using ROMExplorer.Utils;

namespace ROMExplorer.SImg
{
    internal class SparseStream : Stream
    {
        private readonly List<ChunkBase> chunks = new List<ChunkBase>();
        private readonly Stream stream;
        private long length;
        private sparse_header sparseHeader;

        public SparseStream(Stream stream)
        {
            this.stream = stream;
        }

        public static bool Detect(Stream stream)
        {
            var binaryReader = new BinaryReader(stream);
            var sparseHeader = binaryReader.ReadStruct<sparse_header>();
            return sparseHeader.magic == SPARSE_HEADER_MAGIC;
        }

        public bool Open()
        {
            stream.Position = 0;
            var binaryReader = new BinaryReader(stream);
            sparseHeader = binaryReader.ReadStruct<sparse_header>();
            length = sparseHeader.total_blks * sparseHeader.blk_sz;
            if (sparseHeader.magic != SPARSE_HEADER_MAGIC)
                return false;

            stream.Position += sparseHeader.file_hdr_sz - SPARSE_HEADER_LEN;

            for (var i = 0; i < sparseHeader.total_chunks; i++)
            {
                var chunkHeader = binaryReader.ReadStruct<chunk_header>();
                stream.Position += sparseHeader.chunk_hdr_sz - CHUNK_HEADER_LEN;

                switch (chunkHeader.chunk_type)
                {
                    case CHUNK_TYPE_RAW:
                        if (chunkHeader.total_sz != sparseHeader.chunk_hdr_sz +
                            chunkHeader.chunk_sz * sparseHeader.blk_sz)
                        {
                            Debug.WriteLine($"Bogus chunk size for chunk {i}, type Raw");
                            return false;
                        }
                        chunks.Add(new RawChunk(stream.Position, chunkHeader.chunk_sz * sparseHeader.blk_sz));
                        stream.Position += chunkHeader.chunk_sz * sparseHeader.blk_sz;
                        break;

                    case CHUNK_TYPE_DONT_CARE:
                        if (chunkHeader.total_sz != sparseHeader.chunk_hdr_sz)
                        {
                            Debug.WriteLine($"Bogus chunk size for chunk {i}, type Dont Care");
                            return false;
                        }
                        chunks.Add(new SkipChunk(chunkHeader.chunk_sz * sparseHeader.blk_sz));
                        break;

                    case CHUNK_TYPE_CRC32:
                    default:
                        Debug.WriteLine($"Unknown chunk type 0x{chunkHeader.chunk_type:2X}");
                        break;
                }
            }

            return true;
        }

        private abstract class ChunkBase
        {
            protected ChunkBase(long size)
            {
                Size = size;
            }

            public long Size { get; }

            public abstract int Read(byte[] buffer, int offset, int count, Stream stream1, long pos);
        }

        private class RawChunk : ChunkBase
        {
            private readonly long startPositionSrc;

            public RawChunk(long startPositionSrc, long size)
                : base(size)
            {
                this.startPositionSrc = startPositionSrc;
            }

            #region Overrides of ChunkBase

            public override int Read(byte[] buffer, int offset, int count, Stream stream1, long pos)
            {
                if (count > Size - offset)
                    count = (int) (Size - offset);

                stream1.Position = startPositionSrc + pos;
                return stream1.Read(buffer, offset, count);
            }

            #endregion
        }

        private class SkipChunk : ChunkBase
        {
            public SkipChunk(long size)
                : base(size)
            {
            }

            #region Overrides of ChunkBase

            public override int Read(byte[] buffer, int offset, int count, Stream stream1, long pos)
            {
                if (count > Size - offset)
                    count = (int) (Size - offset);
                return count;
            }

            #endregion
        }

        #region structs

        // see https://android.googlesource.com/platform/system/extras/+/ics-mr1-release/ext4_utils/sparse_format.h

#pragma warning disable 0649
        private struct sparse_header
        {
            public uint magic; /* 0xed26ff3a */
            public ushort major_version; /* (0x1) - reject images with higher major versions */
            public ushort minor_version; /* (0x0) - allow images with higer minor versions */
            public ushort file_hdr_sz; /* 28 bytes for first revision of the file format */
            public ushort chunk_hdr_sz; /* 12 bytes for first revision of the file format */
            public uint blk_sz; /* block size in bytes, must be a multiple of 4 (4096) */
            public uint total_blks; /* total blocks in the non-sparse output image */
            public uint total_chunks; /* total chunks in the sparse input image */

            public uint image_checksum; /* CRC32 checksum of the original data, counting "don't care" */
            /* as 0. Standard 802.3 polynomial, use a Public Domain */
            /* table implementation */
        }
#pragma warning restore 0649

        private const uint SPARSE_HEADER_MAGIC = 0xed26ff3a;

        private const ushort CHUNK_TYPE_RAW = 0xCAC1;
        private const ushort CHUNK_TYPE_FILL = 0xCAC2;
        private const ushort CHUNK_TYPE_DONT_CARE = 0xCAC3;
        private const ushort CHUNK_TYPE_CRC32 = 0xCAC4;

        private static readonly uint SPARSE_HEADER_LEN = (uint) Marshal.SizeOf(typeof(sparse_header));

        private static readonly uint CHUNK_HEADER_LEN = (uint) Marshal.SizeOf(typeof(chunk_header));

#pragma warning disable 0649
        private struct chunk_header
        {
            public ushort chunk_type; /* 0xCAC1 -> raw; 0xCAC2 -> fill; 0xCAC3 -> don't care */
            public ushort reserved1;
            public uint chunk_sz; /* in blocks in output image */
            public uint total_sz; /* in bytes of chunk input file including chunk header and data */
        }
#pragma warning restore 0649

        #endregion

        #region Overrides of Stream

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                stream.Dispose();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var c = count;

            foreach (var chunk in chunks)
                if (pos < chunk.Size)
                {
                    var read = chunk.Read(buffer, offset, count, stream, pos);
                    pos = 0;
                    c -= read;

                    if (c == 0)
                    {
                        Position += count;
                        return count;
                    }
                    offset += read;
                }
                else
                {
                    pos -= chunk.Size;
                }

            return 0;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = false;
        public override bool CanWrite { get; } = false;

        public override long Length => length;

        public override long Position { get; set; }

        #endregion
    }
}