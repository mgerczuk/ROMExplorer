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

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using K4os.Compression.LZ4.Streams;
using ROMExplorer.Img;
using ROMExplorer.Properties;
using ROMExplorer.Utils;

namespace ROMExplorer.Lz4
{
    internal class Lz4FileInfo : IFileInfo
    {
        private readonly Stream stream;

        public Lz4FileInfo(string filename)
        {
            stream = new FileStream(filename, FileMode.Open);

            Root = OpenLz4Stream(stream);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            stream.Dispose();
        }

        #endregion

        public static Stream DecodeLz4(Stream stream)
        {
            var decoderStream = LZ4Stream.Decode(stream);
            return TempFileStream.CreateFrom(decoderStream);
        }

        public static DiscDirectoryInfoTreeItemViewModel OpenLz4Stream(Stream stream)
        {
            var decoded = DecodeLz4(stream);

            return ImgFileInfo.OpenImgStream(decoded);
        }

        public class Factory : IFileInfoFactory
        {
            #region Implementation of IFileInfoFactory

            public string Filter { get; } = "LZ4 Files (*.lz4)|*.lz4";

            public IFileInfo Create(string filename)
            {
                return new Lz4FileInfo(filename);
            }

            #endregion
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Implementation of IFileInfo

        public IEnumerable<ArchiveEntryViewModelBase> ArchiveEntries { get; } = null;

        public DiscDirectoryInfoTreeItemViewModel Root { get; }

        #endregion
    }
}