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
using System.IO;
using System.Linq;
using ROMExplorer.Img;
using SharpCompress.Archives.Tar;

namespace ROMExplorer.Tar
{
    internal class TarFileInfo : FileInfoBase
    {
        private readonly List<ArchiveEntryViewModelBase> archiveEntries = new List<ArchiveEntryViewModelBase>();
        private readonly FileStream fileStream;
        private readonly TarArchive tar;

        public TarFileInfo(string filename)
        {
            // TODO: check fileStream cleanup
            fileStream = new FileStream(filename, FileMode.Open);
            tar = TarArchive.Open(fileStream);

            var root0 = new DirectoryArchiveEntryViewModel();
            root0.InitDirectories(tar.Entries.Select(e => e.Key));
            root0.AddEntries(tar.Entries.Select(e => new TarEntryViewModel(this, e)));
            archiveEntries.AddRange(root0.Children);
        }

        #region Implementation of FileInfoBase

        public override IEnumerable<ArchiveEntryViewModelBase> ArchiveEntries => archiveEntries;

        #endregion

        #region Implementation of IDisposable

        public override void Dispose()
        {
            foreach (var entry in archiveEntries)
                entry.Dispose();

            archiveEntries.Clear();
            tar.Dispose();
            fileStream.Dispose();
        }

        #endregion

        public void OpenImgStream(Stream stream1)
        {
            Root = ImgFileInfo.OpenImgStream(stream1);
        }

        public class Factory : IFileInfoFactory
        {
            #region Implementation of IFileInfoFactory

            public string Filter { get; } = "Samsung (*.md5)|*.md5";

            public FileInfoBase Create(string filename)
            {
                return new TarFileInfo(filename);
            }

            #endregion
        }
    }
}