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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Delay;
using DiscUtils;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using Parago.Windows;
using ROMExplorer.Annotations;
using ROMExplorer.Ftf;
using ROMExplorer.Huawei;
using ROMExplorer.Img;
using ROMExplorer.Lz4;
using ROMExplorer.Sin;
using ROMExplorer.Tar;
using ROMExplorer.Utils;
using ROMExplorer.Zip;

namespace ROMExplorer
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable, IDragSource
    {
        private static readonly IFileInfoFactory[] factories =
        {
            new ZipFileInfo.Factory(), 
            new TarFileInfo.Factory(),
            new FtfFileInfo.Factory(),
            new UpdateAppFileInfo.Factory(),
            new ImgFileInfo.Factory(),
            new Lz4FileInfo.Factory(),
            new SinFileInfo.Factory(),
        };

        private List<DirectoryEntryViewModel> entries;
        private FileInfoBase fileInfo;
        private DiscDirectoryInfoTreeItemViewModel selected;
        private string sourceName;

        public string SourceName
        {
            get => sourceName;
            set
            {
                if (value == sourceName)
                    return;
                sourceName = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectFile => new RelayCommand(o =>
        {
            var dialog = new OpenFileDialog
            {
                Filter = string.Join("|", factories.Select(f => f.Filter))
            };

            if (dialog.ShowDialog() == true)
                try
                {
                    ProgressDialog.Execute(
                        Application.Current.MainWindow,
                        $"Open {Path.GetFileName(dialog.FileName)}...",
                        () =>
                        {
                            var factory = factories[dialog.FilterIndex - 1];
                            FileInfo = factory.Create(dialog.FileName);
                            SourceName = dialog.FileName;
                        },
                        new ProgressDialogSettings(false, false, false));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        });

        public FileInfoBase FileInfo
        {
            get => fileInfo;
            private set
            {
                if (Equals(value, fileInfo))
                    return;

                fileInfo?.Dispose();
                fileInfo = null;
                OnPropertyChanged();
                fileInfo = value;
                OnPropertyChanged();
            }
        }

        public DiscDirectoryInfoTreeItemViewModel Selected
        {
            get => selected;
            set
            {
                selected = value;
                SetSelected(value?.Model);
            }
        }

        public IEnumerable<DirectoryEntryViewModel> Entries => entries;

        public ICommand DoubleClick => new RelayCommand(o =>
        {
            using (new WaitCursor())
                SelectEntry(o as DirectoryEntryViewModel);
        });

        #region Implementation of IDisposable

        public void Dispose()
        {
            FileInfo = null;
        }

        #endregion

        private void SelectEntry(DirectoryEntryViewModel entry)
        {
            if (entry?.DiskInfo == null)
                return;

            var path = PathToRoot(entry.DiskInfo).Reverse().Skip(1);

            var info = FileInfo.Root;
            foreach (var p in path)
            {
                info.IsExpanded = true;
                info = info.Directories.First(d => d.Name == p.Name);
            }
            info.IsExpanded = true;
            info.IsSelected = true;
        }

        private static IEnumerable<DiscDirectoryInfo> PathToRoot(DiscDirectoryInfo info)
        {
            while (info != null)
            {
                yield return info;
                info = info.Parent;
            }
        }

        private void SetSelected(DiscDirectoryInfo directoryInfo)
        {
            entries = directoryInfo?.GetDirectories().Select(di => new DirectoryEntryViewModel(di))
                          .Concat(directoryInfo.GetFiles().Select(fi => new DirectoryEntryViewModel(fi)))
                          .ToList() ?? new List<DirectoryEntryViewModel>();
            OnPropertyChanged(nameof(Entries));
        }

        private static IEnumerable<FileInfoEx> GetFileInfos(string path, DirectoryEntryViewModel dirEntry)
        {
            if (dirEntry.FileInfo != null)
                return GetFileInfos(path, dirEntry.FileInfo);

            if (dirEntry.DiskInfo != null)
                return GetFileInfos(path, dirEntry.DiskInfo);

            throw new InvalidOperationException();
        }

        private static IEnumerable<FileInfoEx> GetFileInfos(string path, DiscFileInfo fileInfo)
        {
            return new[] {new FileInfoEx(path, fileInfo)};
        }

        private static IEnumerable<FileInfoEx> GetFileInfos(string path, DiscDirectoryInfo directoryInfo)
        {
            var extendedPath = path == null ? directoryInfo.Name : Path.Combine(path, directoryInfo.Name);

            return directoryInfo.GetDirectories().SelectMany(di => GetFileInfos(extendedPath, di))
                .Concat(directoryInfo.GetFiles().SelectMany(fi => GetFileInfos(extendedPath, fi)));
        }

        private class FileInfoEx
        {
            private readonly DiscFileInfo info;
            private readonly string path;

            public FileInfoEx(string path, DiscFileInfo info)
            {
                this.path = path;
                this.info = info;
            }

            public VirtualFileDataObject.FileDescriptor CreateFileDescriptor()
            {
                return new VirtualFileDataObject.FileDescriptor
                {
                    Name = path == null ? info.Name : Path.Combine(path, info.Name),
                    Length = info.Length,
                    ChangeTimeUtc = info.LastWriteTime,
                    StreamContents = stream =>
                    {
                        using (var src = info.Open(FileMode.Open))
                        {
                            src.CopyTo(stream);
                        }
                    }
                };
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

        #region Implementation of IDragSource

        public void StartDrag(IDragInfo dragInfo)
        {
            var itemCount = dragInfo.SourceItems.Cast<object>().Count();

            dragInfo.Effects = itemCount > 0
                ? DragDropEffects.Copy | DragDropEffects.Move
                : DragDropEffects.None;
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            StartDrag(dragInfo);
            if (dragInfo.Effects != DragDropEffects.None)
            {
                var fileDescriptors = dragInfo.SourceItems.Cast<DirectoryEntryViewModel>()
                    .SelectMany(dirEntry => GetFileInfos(null, dirEntry))
                    .Select(fi => fi.CreateFileDescriptor())
                    .ToArray();

                var virtualFileDataObject = new VirtualFileDataObject();
                virtualFileDataObject.SetData(fileDescriptors);

                // the following does not work when run inside VS debugger
                if (!Debugger.IsAttached)
                    VirtualFileDataObject.DoDragDrop(dragInfo.VisualSource, virtualFileDataObject, dragInfo.Effects);
            }
            // don't run default handler
            return false;
        }

        public void Dropped(IDropInfo dropInfo)
        {
        }

        public void DragCancelled()
        {
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return false;
        }

        #endregion
    }
}