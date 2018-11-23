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
using ROMExplorer.Utils;

namespace ROMExplorer.BlockImg
{
    internal class BlockImgStream : Stream
    {
        private readonly Stream outStream;

        public BlockImgStream(Stream inStream, TransferList transferList)
        {
            outStream = TempFileStream.Create();

            long total = transferList.TotalBlocks * TransferList.BLOCKSIZE;
            var done = 0L;
            foreach (var entry in transferList.Entries)
            {
                done += entry.Perform(inStream, outStream) * TransferList.BLOCKSIZE;
                FileInfoBase.ReportProgress(done, total);
            }
        }

        #region Overrides of Stream

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                outStream.Dispose();
        }

        public override void Flush()
        {
            outStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return outStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            outStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return outStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            outStream.Write(buffer, offset, count);
        }

        public override bool CanRead => outStream.CanRead;

        public override bool CanSeek => outStream.CanSeek;

        public override bool CanWrite => outStream.CanWrite;

        public override long Length => outStream.Length;

        public override long Position
        {
            get => outStream.Position;
            set => outStream.Position = value;
        }

        #endregion
    }
}