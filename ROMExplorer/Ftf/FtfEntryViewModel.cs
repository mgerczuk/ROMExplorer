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
using ROMExplorer.Utils;
using SharpCompress.Archives.Zip;

namespace ROMExplorer.Ftf
{
    internal class FtfEntryViewModel : ArchiveEntryViewModelBase
    {
        private readonly Lazy<Stream> getStream;
        private readonly FtfFileInfo parent;

        public FtfEntryViewModel(FtfFileInfo parent, ZipArchiveEntry e)
        {
            this.parent = parent;
            this.parent = parent;
            Name = e.Key;
            getStream = new Lazy<Stream>(delegate
            {
                using (var srcStream = e.OpenEntryStream())
                {
                    return TempFileStream.CreateFrom(srcStream);
                }
            });
        }

        #region Overrides of ArchiveEntryViewModelBase

        public override void Dispose()
        {
            if (getStream.IsValueCreated)
                getStream.Value.Dispose();
        }

        public override void Select()
        {
            var stream = getStream.Value;
            stream.Position = 0;
            parent.OpenSinStream(stream);
        }

        public override bool IsImage =>
            Name.EndsWith(".sin");

        #endregion
    }
}