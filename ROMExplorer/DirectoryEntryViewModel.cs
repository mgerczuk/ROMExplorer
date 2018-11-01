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
using System.Diagnostics;
using DiscUtils;

namespace ROMExplorer
{
    // View model for folder & file list on the right
    public class DirectoryEntryViewModel
    {
        public DirectoryEntryViewModel(DiscDirectoryInfo diskInfo)
        {
            DiskInfo = diskInfo;
            Name = diskInfo.Name;
        }

        public DirectoryEntryViewModel(DiscFileInfo fileInfo)
        {
            FileInfo = fileInfo;
            Name = fileInfo.Name;
            try
            {
                Size = fileInfo.Length;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Size = null;
            }
        }

        public DiscFileInfo FileInfo { get; }

        public DiscDirectoryInfo DiskInfo { get; }

        public string Name { get; }

        public long? Size { get; }
    }
}