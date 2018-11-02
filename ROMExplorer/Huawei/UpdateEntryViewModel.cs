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
// 

using System;
using System.IO;
using HuaweiUpdateLibrary.Core;

namespace ROMExplorer.Huawei
{
    internal class UpdateEntryViewModel : ArchiveEntryViewModelBase, IDisposable
    {
        private readonly Lazy<Stream> getStream;
        private readonly UpdateAppFileInfo parent;

        public UpdateEntryViewModel(UpdateAppFileInfo parent, string filename, UpdateEntry e)
        {
            this.parent = parent;
            Name = e.FileType;
            getStream = new Lazy<Stream>(() => e.GetDataStream(filename));
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (getStream.IsValueCreated)
                getStream.Value.Dispose();
        }

        #endregion

        #region Overrides of ArchiveEntryViewModelBase

        public override void Select()
        {
            var stream = getStream.Value;
            stream.Position = 0;
            parent.OpenStream(stream);
        }

        #endregion
    }
}