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
using System.IO;

namespace ROMExplorer.Utils
{
    internal class ReadOnlyStream : Stream
    {
        private readonly Stream stream;
        private long position;

        public ReadOnlyStream(Stream stream)
        {
            this.stream = stream;
        }

        #region Overrides of Stream

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                stream.Dispose();
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Seek(offset - Position, SeekOrigin.Current);
                    break;
                case SeekOrigin.Current:
                    if (offset < 0 || offset > int.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(origin), origin, "Cannot seek backwards");

                    if (offset > 0)
                    {
                        stream.Read(new byte[offset], 0, (int) offset);
                        position += offset;
                    }
                    break;
                case SeekOrigin.End:
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = stream.Read(buffer, offset, count);
            position += read;
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => stream.Length;

        public override long Position
        {
            get => position;
            set => Seek(value - position, SeekOrigin.Current);
        }

        #endregion
    }
}