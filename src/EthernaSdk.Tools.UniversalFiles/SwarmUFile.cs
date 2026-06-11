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

using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Manifest;
using Etherna.SwarmSdk.Models;
using Etherna.SwarmSdk.Stores;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.Sdk.Tools.UniversalFiles
{
    public class SwarmUFile(
        ISwarmClient swarmClient,
        UUri fileUri)
        : UFile(fileUri)
    {
        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        protected override async Task<(bool Result, (byte[] ByteArray, Encoding? Encoding)? ContentCache)> ExistsAsync(
            UUri absoluteUri)
        {
            ArgumentNullException.ThrowIfNull(absoluteUri);
            
            if (absoluteUri.UriKind != UUriKind.OnlineAbsolute)
                throw new InvalidOperationException(
                    "Invalid online absolute uri kind. It can't be casted to SwarmAddress");

            // Try to get file head.
            try
            {
                var headers = await swarmClient.TryGetFileHeadersAsync(absoluteUri.OriginalUri).ConfigureAwait(false);
                if (headers is null)
                    return (false, null);
            }
            catch { return (false, null); }
            
            return (true, null);
        }

        protected override async Task<(long Result, (byte[] ByteArray, Encoding? Encoding)? ContentCache)> GetByteSizeAsync(
            UUri absoluteUri)
        {
            ArgumentNullException.ThrowIfNull(absoluteUri);
            
            var size = await swarmClient.TryGetFileSizeAsync(SwarmAddress.FromString(absoluteUri.OriginalUri)).ConfigureAwait(false);
            if (size is null)
                throw new InvalidOperationException();
            return (size.Value, null);
        }

        protected override async Task<(byte[] ByteArray, Encoding? Encoding)> ReadToByteArrayAsync(UUri absoluteUri)
        {
            var (contentStream, encoding) = await ReadToStreamAsync(absoluteUri).ConfigureAwait(false);
            
            // Copy stream to memory stream.
            using var memoryStream = new MemoryStream();
            await contentStream.CopyToAsync(memoryStream).ConfigureAwait(false);
            memoryStream.Position = 0;
            
            var byteArrayContent = memoryStream.ToArray();
            await contentStream.DisposeAsync().ConfigureAwait(false);

            return (byteArrayContent, encoding);
        }

        protected override async Task<(Stream Stream, Encoding? Encoding)> ReadToStreamAsync(UUri absoluteUri)
        {
            ArgumentNullException.ThrowIfNull(absoluteUri);
            
            var result = await swarmClient.GetFileAsync(absoluteUri.OriginalUri).ConfigureAwait(false);
            
            // Try to extract the encoding from the Content-Type header.
            Encoding? contentEncoding = null;
            if (result.ContentHeaders?.ContentType?.CharSet != null)
            {
                try { contentEncoding = Encoding.GetEncoding(result.ContentHeaders.ContentType.CharSet); }
                catch (ArgumentException) { }
            }
            
            return (result.Stream, contentEncoding);
        }

        protected override Task<string?> TryGetFileNameAsync(
            UUri absoluteUri)
        {
            ArgumentNullException.ThrowIfNull(absoluteUri);

            return SwarmAddressResolver.TryGetFileNameAsync(
                SwarmAddress.FromString(absoluteUri.OriginalUri),
                new SwarmClientChunkStore(swarmClient));
        }
    }
}