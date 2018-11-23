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

namespace ROMExplorer.Sin
{
    internal class SinFileInfo : FileInfoBase
    {
        private readonly Stream stream;

        public SinFileInfo(string filename)
        {
            stream = new FileStream(filename, FileMode.Open);

            Root = OpenSinStream(stream);
        }

        public static Stream DecodeSin(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var version = reader.ReadByte();
            switch (version)
            {
                case 3:
                    var tempStream = TempFileStream.Create();
                    SinParserV3.CopyTo(reader,tempStream);
                    return tempStream;
            }

            throw new NotImplementedException();
        }

        public static DiscDirectoryInfoTreeItemViewModel OpenSinStream(Stream stream)
        {
            var decoded = DecodeSin(stream);

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