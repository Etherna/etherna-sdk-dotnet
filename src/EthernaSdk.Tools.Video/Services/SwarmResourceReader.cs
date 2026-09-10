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

using Etherna.Sdk.Tools.UniversalFiles;
using Etherna.Sdk.Tools.UniversalFiles.Extensions;
using Etherna.SwarmSdk.Manifest;
using Etherna.SwarmSdk.Models;
using Etherna.SwarmSdk.Services;
using Etherna.SwarmSdk.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Etherna.Sdk.Tools.Video.Services
{
    /// <summary>
    /// Reads the resources of a published video manifest by their Swarm address.
    /// File contents are read through the file provider when one is given (a Swarm file is then served whole by the
    /// bzz endpoint, one request per file) and through the chunk store otherwise. Entry references are always resolved
    /// through the chunk store, walking the mantaray manifest of each root reference once per reader.
    /// </summary>
    public sealed class SwarmResourceReader(
        IChunkService chunkService,
        IReadOnlyChunkStore chunkStore,
        IUFileProvider? uFileProvider = null)
    {
        // Fields.
        private readonly Dictionary<SwarmReference, ReferencedMantarayManifest> mantarayManifests = new();

        // Methods.
        /// <summary>
        /// Get the content stream of a file, resolving its address with browser semantics (index documents,
        /// redirects to directories).
        /// </summary>
        /// <param name="address">The file address</param>
        /// <returns>The file content stream</returns>
        public async Task<Stream> GetFileStreamAsync(SwarmAddress address)
        {
            if (uFileProvider is null)
                return await chunkService.GetFileStreamFromAddressAsync(
                    address,
                    ManifestPathResolver.BrowserResolver,
                    chunkStore).ConfigureAwait(false);

            var uFile = uFileProvider.BuildNewUFile(new SwarmUUri(address));
            return (await uFile.ReadToStreamAsync().ConfigureAwait(false)).Stream;
        }

        /// <summary>
        /// Resolve the entry reference of an address, walking its mantaray manifest.
        /// </summary>
        /// <param name="address">The resource address</param>
        /// <returns>The resolved reference</returns>
        public async Task<SwarmReference> ResolveReferenceAsync(SwarmAddress address)
        {
            if (!mantarayManifests.TryGetValue(address.Reference, out var mantarayManifest))
            {
                mantarayManifest = await ReferencedMantarayManifest.BuildNewAsync(
                    address.Reference,
                    chunkStore).ConfigureAwait(false);
                mantarayManifests[address.Reference] = mantarayManifest;
            }

            return (await mantarayManifest.GetResourceInfoAsync(
                address.Path,
                ManifestPathResolver.IdentityResolver).ConfigureAwait(false)).Result.Reference;
        }

        /// <summary>
        /// Resolve a reference from a hash, or from an address (root reference + path) through its mantaray manifest.
        /// </summary>
        /// <param name="referenceOrAddress">A hash, or an address</param>
        /// <returns>The resolved reference</returns>
        public Task<SwarmReference> ResolveReferenceAsync(string referenceOrAddress)
        {
            ArgumentNullException.ThrowIfNull(referenceOrAddress);

            if (SwarmHash.IsValidHash(referenceOrAddress))
                return Task.FromResult(new SwarmReference(SwarmHash.FromString(referenceOrAddress), null));
            return ResolveReferenceAsync(SwarmAddress.FromString(referenceOrAddress));
        }
    }
}
