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
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Etherna.Sdk.Tools.UniversalFiles
{
    public class UFileProvider : IUFileProvider
    {
        // Fields.
        private readonly Dictionary<Type, Func<UUri, UFile>> uFileBuilders = new(); //<typeof(UUri), UFile builder>

        // Constructor.
        public UFileProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<UFileProvider>? logger = null)
        {
            Logger = logger ?? NullLogger<UFileProvider>.Instance;
            uFileBuilders[typeof(BasicUUri)] = uuri => new BasicUFile((BasicUUri)uuri, httpClientFactory, Logger);
        }

        // Properties.
        /// <summary>
        /// The logger of the provider, shared with the files it builds
        /// </summary>
        internal ILogger Logger { get; }
        
        // Methods.
        public BasicUFile BuildNewUFile(BasicUUri uuri) =>
            (BasicUFile)BuildNewUFile((UUri)uuri);
        
        public UFile BuildNewUFile(UUri uuri)
        {
            ArgumentNullException.ThrowIfNull(uuri);
            
            var builder = uFileBuilders[uuri.GetType()];
            return builder(uuri);
        }
        
        public void RegisterUUriType<TUUri>(Func<TUUri, UFile> builder) where TUUri : UUri =>
            uFileBuilders[typeof(TUUri)] = uuri => builder((TUUri)uuri);

        public async Task<BasicUFile> ToLocalUFileAsync(
            UFile inputUFile,
            UUriKind allowedUriKinds = UUriKind.All,
            string? baseDirectory = null,
            BasicUUri? outputUUri = null)
        {
            ArgumentNullException.ThrowIfNull(inputUFile);

            // If it's already a local file, skip file retrieve.
            if (inputUFile is BasicUFile basicUFile &&
                (basicUFile.FileUri.UriKind & allowedUriKinds & UUriKind.Local) != 0)
                return basicUFile;
            
            if (outputUUri != null && outputUUri.UriKind != UUriKind.LocalAbsolute)
                throw new InvalidOperationException("If provided, output uuri must be local and absolute");

            if (outputUUri is null)
            {
                var fileName = await inputUFile.TryGetFileNameAsync(allowedUriKinds, baseDirectory).ConfigureAwait(false);
                if (fileName is null)
                    outputUUri = new BasicUUri(Path.GetTempFileName(), UUriKind.LocalAbsolute);
                else
                {
                    var tmpDirectory = Directory.CreateDirectory(Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString()));
                    outputUUri = new BasicUUri(Path.Join(tmpDirectory.FullName, fileName), UUriKind.LocalAbsolute);
                }
            }

            using var inputFileStream = (await inputUFile.ReadToStreamAsync(allowedUriKinds, baseDirectory).ConfigureAwait(false)).Stream;
            using var outputFileStream = new FileStream(outputUUri.ToAbsoluteUri().OriginalUri, FileMode.Create);
            await inputFileStream.CopyToAsync(outputFileStream).ConfigureAwait(false);
            Logger.UFileCopiedToLocalFile(inputUFile.FileUri.OriginalUri, outputFileStream.Name);

            return BuildNewUFile(outputUUri);
        }
    }
}