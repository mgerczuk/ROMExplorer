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

namespace ROMExplorer
{
    internal class DirectoryArchiveEntryViewModel : ArchiveEntryViewModelBase 
    {
        private static readonly StringArrayEqualityComparer comparer = new StringArrayEqualityComparer();

        #region Overrides of ArchiveEntryViewModelBase

        public override void Select()
        {
        }

        public override bool IsImage { get; } = false;

        public override void Dispose()
        {
        }

        #endregion

        public void InitDirectories(IEnumerable<string> fileNames)
        {
            var paths = fileNames.Select(GetPath).Where(p => p.Length > 0).Distinct(comparer).ToList();
            InitPaths(paths);
        }

        private void InitPaths(IList<string[]> paths)
        {
            foreach (var path in paths.Select(p => p[0]).Distinct())
            {
                var dir = new DirectoryArchiveEntryViewModel {Name = path};
                var x = paths.Where(p => p[0] == path && p.Length > 1).Select(p => p.Skip(1).ToArray()).ToList();
                dir.InitPaths(x);
                Children.Add(dir);
            }
        }

        private static string[] GetPath(string name)
        {
            var parts = name.Split('/');
            return parts.Take(parts.Length - 1).ToArray();
        }

        private DirectoryArchiveEntryViewModel GetDirectory(string[] parts)
        {
            if (parts.Length == 1)
                return this;

            return Children.OfType<DirectoryArchiveEntryViewModel>().First(d => d.Name == parts[0])
                .GetDirectory( parts.Skip(1).ToArray());
        }

        private class StringArrayEqualityComparer : IEqualityComparer<string[]>
        {
            #region Implementation of IEqualityComparer<in string[]>

            public bool Equals(string[] x, string[] y)
            {
                if (x == null)
                    return y == null;
                return x.SequenceEqual(y);
            }

            public int GetHashCode(string[] obj)
            {
                return (int) (obj.Select(s => (long) s.GetHashCode()).Sum() % int.MaxValue);
            }

            #endregion
        }

        public void AddEntries(IEnumerable<ArchiveEntryViewModelBase> entries)
        {
            foreach (var entry in entries)
            {
                var parts = entry.Name.Split('/');
                var dir = GetDirectory(parts);
                entry.Name = parts.Last();
                dir.Children.Add(entry);
            }
        }
    }
}