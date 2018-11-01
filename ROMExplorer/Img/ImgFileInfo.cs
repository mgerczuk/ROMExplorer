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
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using DiscUtils.Ext;
using ROMExplorer.Properties;
using ROMExplorer.SImg;

namespace ROMExplorer.Img
{
    internal class ImgFileInfo : IFileInfo
    {
        private readonly Stream stream;

        public ImgFileInfo(string filename)
        {
            stream = new FileStream(filename, FileMode.Open);

            Root = OpenImgStream(stream);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            stream.Dispose();
        }

        #endregion

        public static DiscDirectoryInfoTreeItemViewModel OpenImgStream(Stream stream)
        {
            if (SparseStream.Detect(stream))
            {
                var sparseStream = new SparseStream(stream);
                if (!sparseStream.Open())
                    throw new Exception("Wrong format");
                stream = sparseStream;
            }
            var fileSystem = new ExtFileSystem(stream);

            return new DiscDirectoryInfoTreeItemViewModel(fileSystem.Root);
        }

        public class Factory : IFileInfoFactory
        {
            #region Implementation of IFileInfoFactory

            public string Filter { get; } = "Image Files (*.img,*.ext4)|*.img;*.ext4";

            public IFileInfo Create(string filename)
            {
                return new ImgFileInfo(filename);
            }

            #endregion
        }

        #region Overrides of FileInfoBase

        public IEnumerable<ArchiveEntryViewModelBase> ArchiveEntries { get; } = null;

        public DiscDirectoryInfoTreeItemViewModel Root { get; }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}