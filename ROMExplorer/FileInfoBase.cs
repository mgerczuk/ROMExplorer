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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Parago.Windows;
using ROMExplorer.Annotations;

namespace ROMExplorer
{
    public abstract class FileInfoBase : INotifyPropertyChanged, IDisposable
    {
        private bool isPopupOpen;
        private DiscDirectoryInfoTreeItemViewModel root;
        private ArchiveEntryViewModelBase selectedArchiveEntry;

        public abstract IEnumerable<ArchiveEntryViewModelBase> ArchiveEntries { get; }

        public ArchiveEntryViewModelBase SelectedArchiveEntry
        {
            get => selectedArchiveEntry;
            set
            {
                if (Equals(value, selectedArchiveEntry)) return;
                var oldEntry = selectedArchiveEntry;
                IsPopupOpen = false;

                selectedArchiveEntry = value;
                OnPropertyChanged();
                try
                {
                    if (value != null)
                    {
                        ProgressDialog.Execute(
                            Application.Current.MainWindow,
                            $"Extracting {value.Name}...",
                            () => value.Select(),
                            new ProgressDialogSettings(false, false, false));
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);

                    selectedArchiveEntry = oldEntry;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPopupOpen
        {
            get => isPopupOpen;
            set
            {
                if (value == isPopupOpen) return;
                isPopupOpen = value;
                OnPropertyChanged();
            }
        }

        public DiscDirectoryInfoTreeItemViewModel Root
        {
            get => root;
            protected set
            {
                if (Equals(value, root)) return;
                root = value;
                OnPropertyChanged();
            }
        }

        #region Implementation of IDisposable

        public abstract void Dispose();

        #endregion

        public static void ReportProgress(long done, long total)
        {
            var percent = 100L * done / total;
            ProgressDialog.Current?.ReportWithCancellationCheck((int) percent, "");
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