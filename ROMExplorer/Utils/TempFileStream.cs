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

using System.IO;

namespace ROMExplorer.Utils
{
    internal class TempFileStream : FileStream
    {
        private readonly string path;

        private TempFileStream(string path, FileMode mode, FileAccess access)
            : base(path, mode, access)
        {
            this.path = path;
        }

        public static Stream CreateFrom(Stream srcStream)
        {
            var destStream = new TempFileStream(GetTempFileName(), FileMode.Create, FileAccess.ReadWrite);
            srcStream.CopyTo(destStream);
            destStream.Position = 0;
            return destStream;
        }

        public static Stream Create()
        {
            return new TempFileStream(GetTempFileName(), FileMode.Create, FileAccess.ReadWrite);
        }

        #region Overrides of FileStream

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                File.Delete(path);
        }

        #endregion

        public static void Cleanup()
        {
            try
            {
                Directory.Delete(GetTempDirectory(), true);
            }
            catch
            {
            }
            Directory.CreateDirectory(GetTempDirectory());
        }

        private static string GetTempDirectory()
        {
            return Path.Combine(Path.GetTempPath(), "ROMExplorer");
        }

        private static string GetTempFileName()
        {
            while (true)
            {
                var filename = Path.Combine(GetTempDirectory(), Path.GetRandomFileName());
                if (!File.Exists(filename))
                    return filename;
            }
        }
    }
}