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
using HuaweiUpdateLibrary.Core;
using ROMExplorer.Img;

namespace ROMExplorer.Huawei
{
    internal class UpdateAppFileInfo : FileInfoBase
    {
        private readonly IList<UpdateEntryViewModel> archiveEntries;

        public UpdateAppFileInfo(string filename)
        {
            var updateFile = UpdateFile.Open(filename);

            archiveEntries = updateFile.Select(e => new UpdateEntryViewModel(this, filename, e))
                .ToList();
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
        }

        #endregion

        public void OpenStream(Stream stream)
        {
            Root = ImgFileInfo.OpenImgStream(stream);
        }

        public class Factory : IFileInfoFactory
        {
            #region Implementation of IFileInfoFactory

            public string Filter { get; } = "Huawei UPDATE.APP File|*.app";

            public FileInfoBase Create(string filename)
            {
                return new UpdateAppFileInfo(filename);
            }

            #endregion
        }
    }
}