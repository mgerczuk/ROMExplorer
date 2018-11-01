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
using System.Linq;
using DiscUtils;

namespace ROMExplorer
{
    // View model for folder on the tree view on the left
    public class DiscDirectoryInfoTreeItemViewModel
    {
        private readonly IList<DiscDirectoryInfoTreeItemViewModel> directories;

        public DiscDirectoryInfoTreeItemViewModel(DiscDirectoryInfo model)
        {
            Model = model;
            directories = model.GetDirectories().Select(di => new DiscDirectoryInfoTreeItemViewModel(di)).ToList();
        }

        public DiscDirectoryInfo Model { get; }

        public IEnumerable<DiscDirectoryInfoTreeItemViewModel> Directories => directories;

        public string Name => Model.Name;

        public bool IsExpanded { get; set; }

        public bool IsSelected { get; set; }
    }
}