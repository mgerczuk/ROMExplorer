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
using System.IO.Compression;
using Brotli;
using ROMExplorer.BlockImg;
using ROMExplorer.Utils;
using ZipArchiveEntry = SharpCompress.Archives.Zip.ZipArchiveEntry;

namespace ROMExplorer.Zip
{
    internal class ZipEntryViewModel : ArchiveEntryViewModelBase, IDisposable
    {
        private readonly Lazy<Stream> getStream;
        private readonly ZipFileInfo parent;
        private readonly long totalSize;

        public ZipEntryViewModel(ZipFileInfo parent, ZipArchiveEntry e)
        {
            this.parent = parent;
            totalSize = e.Size;
            Name = e.Key;
            getStream = new Lazy<Stream>(delegate
            {
                var transferList = parent.GetTransferList();
                using (var srcStream = e.OpenEntryStream())
                {
                    if (Name.EndsWith(".new.dat"))
                        return new BlockImgStream(srcStream, transferList);
                    if (Name.EndsWith(".new.dat.br"))
                        return new BlockImgStream(
                            new BrotliStream(srcStream, CompressionMode.Decompress), transferList);

                    return TempFileStream.CreateFrom(srcStream, done => FileInfoBase.ReportProgress(done, totalSize));
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

        public override bool IsImage =>
            Name.EndsWith(".img") || Name.EndsWith(".new.dat") || Name.EndsWith(".new.dat.br");

        #endregion
    }
}