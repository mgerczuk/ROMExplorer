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
using System.IO;
using K4os.Compression.LZ4;
using ROMExplorer.SImg;
using ROMExplorer.Utils;

namespace ROMExplorer.Sin
{
    internal class SonySparseStream : SparseStream
    {
        private const ushort CHUNK_TYPE_LZ4 = 0xCAC5;

        public SonySparseStream(Stream stream) : base(stream)
        {
        }

        public bool CopyToEx(Stream outStream, Action callback)
        {
            var binaryReader = new BinaryReader(InStream);
            var sparseHeader = binaryReader.ReadStruct<sparse_header>();
            if (sparseHeader.magic != SPARSE_HEADER_MAGIC)
                return false;

            InStream.Position += sparseHeader.file_hdr_sz - SPARSE_HEADER_LEN;

            outStream.Position = 0L;
            for (var i = 0; i < sparseHeader.total_chunks; i++)
            {
                var chunkHeader = binaryReader.ReadStruct<chunk_header>();
                InStream.Position += sparseHeader.chunk_hdr_sz - CHUNK_HEADER_LEN;

                switch (chunkHeader.chunk_type)
                {
                    case CHUNK_TYPE_RAW:
                        if (chunkHeader.total_sz != sparseHeader.chunk_hdr_sz +
                            chunkHeader.chunk_sz * sparseHeader.blk_sz)
                            throw new Exception($"Bogus chunk size for chunk {i}, type Raw");

                        for (var j = 0; j < chunkHeader.chunk_sz; j++)
                        {
                            var rawBuf = binaryReader.ReadBytes((int) sparseHeader.blk_sz);
                            outStream.Write(rawBuf, 0, (int) sparseHeader.blk_sz);
                        }
                        break;

                    case CHUNK_TYPE_LZ4:
                        if (chunkHeader.total_sz - sparseHeader.chunk_hdr_sz >
                            chunkHeader.chunk_sz * sparseHeader.blk_sz)
                            throw new Exception($"Bogus chunk size for chunk {i}, type LZ4");

                        var compressed =
                            binaryReader.ReadBytes((int) (chunkHeader.total_sz - sparseHeader.chunk_hdr_sz));
                        var uncompressed = new byte[chunkHeader.chunk_sz * sparseHeader.blk_sz];
                        LZ4Codec.Decode(compressed, uncompressed);
                        outStream.Write(uncompressed, 0, uncompressed.Length);
                        break;

                    case CHUNK_TYPE_DONT_CARE:
                        if (chunkHeader.total_sz != sparseHeader.chunk_hdr_sz)
                            throw new Exception($"Bogus chunk size for chunk {i}, type Dont Care");

                        outStream.Position += chunkHeader.chunk_sz * sparseHeader.blk_sz;
                        break;

                    default:
                        throw new Exception($"Unexpected chunk type 0x{chunkHeader.chunk_type:X}");
                }

                callback();
            }

            return true;
        }
    }
}