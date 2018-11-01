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
using ROMExplorer.Img;
using ROMExplorer.Properties;
using SharpCompress.Archives.Tar;

namespace ROMExplorer.Tar
{
    internal class TarFileInfo : IFileInfo
    {
        private readonly List<TarEntryViewModel> archiveEntries = new List<TarEntryViewModel>();
        private DiscDirectoryInfoTreeItemViewModel root;

        public TarFileInfo(string filename)
        {
            // TODO: check fileStream cleanup
            var fileStream = new FileStream(filename, FileMode.Open);
            using (var x = TarArchive.Open(fileStream))
            {
                archiveEntries.AddRange(x.Entries.Select(e => new TarEntryViewModel(this, e)));
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            foreach (var entry in archiveEntries)
                entry.Dispose();

            archiveEntries.Clear();
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

            public IFileInfo Create(string filename)
            {
                return new TarFileInfo(filename);
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
            get { return root; }
            private set
            {
                if (Equals(root, value))
                    return;

                root = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}