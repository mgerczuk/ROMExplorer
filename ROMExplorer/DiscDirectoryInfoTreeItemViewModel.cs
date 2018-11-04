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
using System.Linq;
using System.Runtime.CompilerServices;
using DiscUtils;
using ROMExplorer.Annotations;

namespace ROMExplorer
{
    // View model for folder on the tree view on the left
    public class DiscDirectoryInfoTreeItemViewModel : INotifyPropertyChanged
    {
        private readonly IList<DiscDirectoryInfoTreeItemViewModel> directories;
        private bool isExpanded;
        private bool isSelected;

        public DiscDirectoryInfoTreeItemViewModel(DiscDirectoryInfo model)
        {
            Model = model;
            directories = model.GetDirectories().Select(di => new DiscDirectoryInfoTreeItemViewModel(di)).ToList();
        }

        public DiscDirectoryInfo Model { get; }

        public IEnumerable<DiscDirectoryInfoTreeItemViewModel> Directories => directories;

        public string Name => Model.Name;

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value == isExpanded) return;
                isExpanded = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value == isSelected) return;
                isSelected = value;
                OnPropertyChanged();
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}