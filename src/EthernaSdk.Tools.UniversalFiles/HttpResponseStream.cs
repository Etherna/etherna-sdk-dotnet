// Copyright 2020-present Etherna SA
// This file is part of Etherna SDK .Net.
//
// Etherna SDK .Net is free software: you can redistribute it and/or modify it under the terms of the
// GNU Lesser General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// Etherna SDK .Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Etherna SDK .Net.
// If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.Tools.UniversalFiles
{
    /// <summary>
    /// The content stream of an HTTP response, that disposes the response together with itself
    /// </summary>
    internal sealed class HttpResponseStream(
        Stream contentStream,
        HttpResponseMessage response)
        : Stream
    {
        // Properties.
        public override bool CanRead => contentStream.CanRead;
        public override bool CanSeek => contentStream.CanSeek;
        public override bool CanWrite => contentStream.CanWrite;
        public override long Length => contentStream.Length;
        public override long Position
        {
            get => contentStream.Position;
            set => contentStream.Position = value;
        }

        // Methods.
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) =>
            contentStream.CopyToAsync(destination, bufferSize, cancellationToken);

        public override void Flush() => contentStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) =>
            contentStream.Read(buffer, offset, count);

        public override int Read(Span<byte> buffer) =>
            contentStream.Read(buffer);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            contentStream.ReadAsync(buffer, offset, count, cancellationToken);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            contentStream.ReadAsync(buffer, cancellationToken);

        public override long Seek(long offset, SeekOrigin origin) =>
            contentStream.Seek(offset, origin);

        public override void SetLength(long value) =>
            contentStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) =>
            contentStream.Write(buffer, offset, count);

        // Protected methods.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                contentStream.Dispose();
                response.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
