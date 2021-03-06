﻿// 
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

using SharpCompress.Archives.Zip;

namespace ROMExplorer.Ftf
{
    internal class FtfEntryViewModel : ArchiveEntryViewModelBase
    {
        private readonly ZipArchiveEntry e;
        private readonly FtfFileInfo parent;
        private readonly long totalSize;

        public FtfEntryViewModel(FtfFileInfo parent, ZipArchiveEntry e)
        {
            this.e = e;
            this.parent = parent;
            totalSize = e.Size; // openEntryStream clears Size?!?
            Name = e.Key;
        }

        #region Overrides of ArchiveEntryViewModelBase

        public override void Dispose()
        {
        }

        public override void Select()
        {
            using (var srcStream = e.OpenEntryStream())
            {
                parent.OpenSinStream(srcStream, totalSize);
            }
        }

        public override bool IsImage =>
            Name.EndsWith(".sin");

        #endregion
    }
}