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
using System.Linq;
using System.Runtime.CompilerServices;
using ROMExplorer.Annotations;
using ROMExplorer.BlockImg;
using ROMExplorer.Img;
using SharpCompress.Archives.Zip;

namespace ROMExplorer.Zip
{
    internal class ZipFileInfo : IFileInfo
    {
        private readonly List<ZipEntryViewModel> archiveEntries = new List<ZipEntryViewModel>();
        private readonly FileStream fileStream;
        private DiscDirectoryInfoTreeItemViewModel root;
        private readonly ZipArchive zip;

        public ZipFileInfo(string filename)
        {
            fileStream = new FileStream(filename, FileMode.Open);
            zip = ZipArchive.Open(fileStream);

            archiveEntries.AddRange(zip.Entries.Select(e => new ZipEntryViewModel(this, e)));
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            foreach (var entry in archiveEntries)
                entry.Dispose();

            archiveEntries.Clear();
            zip.Dispose();
            fileStream.Dispose();
        }

        #endregion

        public void OpenImgStream(Stream stream1)
        {
            Root = ImgFileInfo.OpenImgStream(stream1);
        }

        public TransferList GetTransferList()
        {
            var transferListEntry = zip.Entries.FirstOrDefault(e => e.Key == "system.transfer.list");
            if (transferListEntry == null)
                return null;

            using (var entryStream = transferListEntry.OpenEntryStream())
                return new TransferList(entryStream);
        }

        public class Factory : IFileInfoFactory
        {
            #region Implementation of IFileInfoFactory

            public string Filter { get; } = "Zip Files (*.zip)|*.zip";

            public IFileInfo Create(string filename)
            {
                return new ZipFileInfo(filename);
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

        public IEnumerable<ArchiveEntryViewModelBase> ArchiveEntries => archiveEntries;

        public DiscDirectoryInfoTreeItemViewModel Root
        {
            get => root;
            private set
            {
                if (Equals(value, root)) return;
                root = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}