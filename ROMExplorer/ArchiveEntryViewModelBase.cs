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

using System;
using System.Collections.Generic;

namespace ROMExplorer
{
    // Base view model class for items in "Archive Entries" combo box
    public abstract class ArchiveEntryViewModelBase : IDisposable
    {
        public string Name { get; set; }

        public IList<ArchiveEntryViewModelBase> Children { get; } = new List<ArchiveEntryViewModelBase>();

        public abstract void Select();

        public abstract bool IsImage { get; }

        #region Implementation of IDisposable

        public abstract void Dispose();

        #endregion
    }
}