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
using Etherna.Sdk.Tools.Video.Models;
using Etherna.SwarmSdk.Hashing.Postage;
using Etherna.SwarmSdk.Models;
using Etherna.SwarmSdk.Stores;
using System.Threading.Tasks;

namespace Etherna.Sdk.Tools.Video.Services
{
    public interface IVideoManifestService
    {
        Task<SwarmReference> CreateVideoManifestChunksAsync(
            VideoManifest manifest,
            string chunksDirectory,
            bool createDirectory = true,
            IPostageStampIssuer? postageStampIssuer = null);

        /// <summary>
        /// Get a published video manifest, reading every resource through the chunk store.
        /// </summary>
        /// <param name="manifestReference">The manifest root reference</param>
        /// <param name="chunkStore">The chunk store</param>
        /// <returns>The published video manifest, with its validation errors</returns>
        Task<PublishedVideoManifest> GetPublishedVideoManifestAsync(
            SwarmReference manifestReference,
            IReadOnlyChunkStore chunkStore);

        /// <summary>
        /// Get a published video manifest, reading file contents through the file provider and resolving entry
        /// references through the chunk store. A Swarm file provider serves each file whole from the bzz endpoint,
        /// instead of composing it chunk by chunk.
        /// </summary>
        /// <param name="manifestReference">The manifest root reference</param>
        /// <param name="chunkStore">The chunk store, resolving the entry references</param>
        /// <param name="uFileProvider">The file provider, reading the file contents</param>
        /// <returns>The published video manifest, with its validation errors</returns>
        Task<PublishedVideoManifest> GetPublishedVideoManifestAsync(
            SwarmReference manifestReference,
            IReadOnlyChunkStore chunkStore,
            IUFileProvider uFileProvider);
    }
}