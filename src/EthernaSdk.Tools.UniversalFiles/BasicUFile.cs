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

using Etherna.Sdk.Tools.UniversalFiles.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.Sdk.Tools.UniversalFiles
{
    public class BasicUFile(
        BasicUUri fileUri,
        IHttpClientFactory httpClientFactory,
        ILogger? logger = null)
        : UFile(fileUri)
    {
        // Fields.
        private readonly ILogger logger = logger ?? NullLogger.Instance;

        // Protected methods.
        protected override async Task<(bool Result, (byte[] ByteArray, Encoding? Encoding)? ContentCache)> ExistsAsync(
            UUri absoluteUri)
        {
            ArgumentNullException.ThrowIfNull(absoluteUri);
            
            switch (absoluteUri.UriKind)
            {
                case UUriKind.LocalAbsolute:
                    return (File.Exists(absoluteUri.OriginalUri) || Directory.Exists(absoluteUri.OriginalUri), null);

                case UUriKind.OnlineAbsolute:
                    // A successful HEAD request finds the resource, with or without its byte size.
                    var (isHeadSuccessful, _) = await TryGetOnlineHeadAsync(absoluteUri.OriginalUri).ConfigureAwait(false);
                    if (isHeadSuccessful)
                        return (true, null);

                    // Otherwise, try to download it.
                    var onlineContent = await TryGetOnlineAsByteArrayAsync(absoluteUri.OriginalUri).ConfigureAwait(false);
                    return (onlineContent != null, onlineContent);

                default: throw new InvalidOperationException(
                    "Invalid absolute uri kind. It should be well defined and absolute");
            }
        }

        protected override async Task<(long Result, (byte[] ByteArray, Encoding? Encoding)? ContentCache)> GetByteSizeAsync(
            UUri absoluteUri)
        {
            ArgumentNullException.ThrowIfNull(absoluteUri);
            
            switch (absoluteUri.UriKind)
            {
                case UUriKind.LocalAbsolute:
                    return (new FileInfo(absoluteUri.OriginalUri).Length, null);

                case UUriKind.OnlineAbsolute:
                    // Try to get resource byte size with a HEAD request.
                    var (_, byteSize) = await TryGetOnlineHeadAsync(absoluteUri.OriginalUri).ConfigureAwait(false);
                    if (byteSize.HasValue)
                        return (byteSize.Value, null);

                    // Otherwise, try to download it.
                    var onlineContent = await TryGetOnlineAsByteArrayAsync(absoluteUri.OriginalUri).ConfigureAwait(false) ??
                                        throw new IOException($"Can't retrieve online resource at {absoluteUri.OriginalUri.RemoveUriSecrets()}");
                    return (onlineContent.ByteArray.LongLength, onlineContent);

                default: throw new InvalidOperationException(
                    "Invalid absolute uri kind. It should be well defined and absolute");
            }
        }

        protected override async Task<(byte[] ByteArray, Encoding? Encoding)> ReadToByteArrayAsync(
            UUri absoluteUri)
        {
            ArgumentNullException.ThrowIfNull(absoluteUri);
            
            switch (absoluteUri.UriKind)
            {
                case UUriKind.LocalAbsolute:
                    return (await File.ReadAllBytesAsync(absoluteUri.OriginalUri).ConfigureAwait(false), null);

                case UUriKind.OnlineAbsolute:
                    return await TryGetOnlineAsByteArrayAsync(absoluteUri.OriginalUri).ConfigureAwait(false) ??
                           throw new IOException($"Can't retrieve online resource at {absoluteUri.OriginalUri.RemoveUriSecrets()}");

                default: throw new InvalidOperationException(
                    "Invalid absolute uri kind. It should be well defined and absolute");
            }
        }

        protected override async Task<(Stream Stream, Encoding? Encoding)> ReadToStreamAsync(
            UUri absoluteUri)
        {
            ArgumentNullException.ThrowIfNull(absoluteUri);
            
            switch (absoluteUri.UriKind)
            {
                case UUriKind.LocalAbsolute:
                    return (File.OpenRead(absoluteUri.OriginalUri), null);
                
                case UUriKind.OnlineAbsolute:
                    return await TryGetOnlineAsStreamAsync(absoluteUri.OriginalUri).ConfigureAwait(false) ??
                           throw new IOException($"Can't retrieve online resource at {absoluteUri.OriginalUri.RemoveUriSecrets()}");
                
                default: throw new InvalidOperationException(
                    "Invalid absolute uri kind. It should be well defined and absolute");
            }
        }

        protected override Task<string?> TryGetFileNameAsync(UUri absoluteUri)
        {
            ArgumentNullException.ThrowIfNull(absoluteUri);
            
            if (absoluteUri.OriginalUri.EndsWith('/') ||
                absoluteUri.OriginalUri.EndsWith('\\'))
                return Task.FromResult<string?>(null);
            return Task.FromResult<string?>(absoluteUri.OriginalUri.Split('/', '\\').Last());
        }

        // Helpers.
        private static Encoding? TryGetContentEncoding(HttpResponseMessage response)
        {
            // Try to extract the encoding from the Content-Type header.
            var charSet = response.Content.Headers.ContentType?.CharSet;
            if (charSet is null)
                return null;

            try { return Encoding.GetEncoding(charSet); }
            catch (ArgumentException) { return null; }
        }

        private async Task<(byte[] ByteArray, Encoding? Encoding)?> TryGetOnlineAsByteArrayAsync(
            string onlineAbsoluteUri)
        {
            // The whole content is read with the response, so a download failed midway is a failed request.
            using var response = await TryGetOnlineResponseAsync(
                onlineAbsoluteUri,
                HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            if (response is null)
                return null;

            return (await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false),
                TryGetContentEncoding(response));
        }

        private async Task<(Stream Stream, Encoding? Encoding)?> TryGetOnlineAsStreamAsync(
            string onlineAbsoluteUri)
        {
            // Only the headers are read with the response: the content is read by the caller, as it arrives,
            // through a stream that disposes the response with itself.
            var response = await TryGetOnlineResponseAsync(
                onlineAbsoluteUri,
                HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            if (response is null)
                return null;

            var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return (new HttpResponseStream(contentStream, response), TryGetContentEncoding(response));
        }

        /// <summary>
        /// Send a HEAD request of the online file
        /// </summary>
        /// <returns>If the response is successful, and the byte size it declares</returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        private async Task<(bool IsSuccessful, long? ByteSize)> TryGetOnlineHeadAsync(string onlineAbsoluteUri)
        {
            try
            {
                using var httpClient = httpClientFactory.CreateClient();
                using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Head, onlineAbsoluteUri);
                using var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

                // A refused response declares the length of its error page, not the one of the file.
                if (response.IsSuccessStatusCode)
                    return (true, response.Content.Headers.ContentLength);

                logger.OnlineFileHeadRequestRefused(onlineAbsoluteUri, response.StatusCode);
            }
            catch (Exception e)
            {
                logger.OnlineFileHeadRequestFailed(onlineAbsoluteUri, e);
            }

            return (false, null);
        }

        /// <summary>
        /// Send a GET request of the online file
        /// </summary>
        /// <returns>The response when successful, null when the request is refused or fails</returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        [SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings")]
        private async Task<HttpResponseMessage?> TryGetOnlineResponseAsync(
            string onlineAbsoluteUri,
            HttpCompletionOption completionOption)
        {
            try
            {
                // Disposing the client doesn't end the reading of a response content: the handler and its
                // connections belong to the factory.
                using var httpClient = httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(onlineAbsoluteUri, completionOption).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                    return response;

                logger.OnlineFileRequestRefused(onlineAbsoluteUri, response.StatusCode);
                response.Dispose();
                return null;
            }
            catch (Exception e)
            {
                logger.OnlineFileRequestFailed(onlineAbsoluteUri, e);
                return null;
            }
        }
    }
}