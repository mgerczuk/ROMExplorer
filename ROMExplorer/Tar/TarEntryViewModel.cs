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
using ROMExplorer.Lz4;
using ROMExplorer.Utils;
using SharpCompress.Archives.Tar;

namespace ROMExplorer.Tar
{
    internal class TarEntryViewModel : ArchiveEntryViewModelBase, IDisposable
    {
        private readonly Lazy<Stream> getStream;
        private readonly TarFileInfo parent;

        public TarEntryViewModel(TarFileInfo parent, TarArchiveEntry e)
        {
            this.parent = parent;
            Name = e.Key;
            getStream = new Lazy<Stream>(delegate
            {
                using (var srcStream = e.OpenEntryStream())
                {
                    if (Name.EndsWith(".img.lz4"))
                        return Lz4FileInfo.DecodeLz4(srcStream);

                    return TempFileStream.CreateFrom(srcStream);
                }
            });
        }

        #region Implementation of IDisposable

        public override void Dispose()
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
            parent.OpenImgStream(stream);
        }

        public override bool IsImage => Name.EndsWith(".img") || Name.EndsWith(".img.lz4");

        #endregion
    }
}