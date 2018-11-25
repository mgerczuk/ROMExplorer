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
using ROMExplorer.Img;
using ROMExplorer.Utils;
using SharpCompress.Readers.Tar;

namespace ROMExplorer.Sin
{
    internal class SinFileInfo : FileInfoBase
    {
        private readonly Stream stream;

        public SinFileInfo(string filename)
        {
            stream = new FileStream(filename, FileMode.Open);

            Root = OpenSinStream(stream, stream.Length);
        }

        public static Stream DecodeSin(Stream stream, long totalSize)
        {
            var rewindable = new RewindableStream(stream);
            rewindable.StartRecording();
            var reader = new BinaryReader(rewindable);
            var version = reader.ReadByte();
            switch (version)
            {
                case 1:
                case 2:
                    throw new Exception($"SIN version 1 and 2 not supported.");

                case 3:
                    rewindable.Rewind(true);
                    var tempStream = TempFileStream.Create();
                    SinParserV3.CopyTo(reader, tempStream);
                    return tempStream;
            }

            rewindable.Rewind(true);
            return DecodeTarSin(rewindable, totalSize);
        }

        public static Stream DecodeTarSin(Stream stream, long totalSize)
        {
            var tarStream = new ReadOnlyStream(stream); // to track Position
            var tarReader = TarReader.Open(tarStream);

            var outStream = TempFileStream.Create();
            while (tarReader.MoveToNextEntry())
                using (var s = new ReadOnlyStream(tarReader.OpenEntryStream()))
                {
                    var sparseStream = new SonySparseStream(s);
                    sparseStream.CopyToEx(outStream, () => { ReportProgress(tarStream.Position, totalSize); });
                }

            return outStream;
        }

        public static DiscDirectoryInfoTreeItemViewModel OpenSinStream(Stream stream, long totalSize)
        {
            var decoded = DecodeSin(stream, totalSize);

            return ImgFileInfo.OpenImgStream(decoded);
        }

        public class Factory : IFileInfoFactory
        {
            #region Implementation of IFileInfoFactory

            public string Filter { get; } = "SIN Files (*.sin)|*.sin";

            public FileInfoBase Create(string filename)
            {
                return new SinFileInfo(filename);
            }

            #endregion
        }

        #region Overrides of FileInfoBase

        public override IEnumerable<ArchiveEntryViewModelBase> ArchiveEntries { get; } = null;

        public override void Dispose()
        {
            stream.Dispose();
        }

        #endregion
    }
}