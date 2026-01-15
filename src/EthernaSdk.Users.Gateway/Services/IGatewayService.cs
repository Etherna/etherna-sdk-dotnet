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

using Etherna.BeeNet.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Etherna.Sdk.Users.Gateway.Services
{
    public interface IGatewayService
    {
        // Methods.
        Task ChunksBulkUploadAsync(
            SwarmChunk[] chunks,
            PostageBatchId batchId);
        
        /// <summary>
        /// Create a new batch.
        /// </summary>
        /// <param name="amount">amount</param>
        /// <param name="batchDepth">batch depth</param>
        /// <param name="onWaitingBatchCreation">event callback</param>
        /// <param name="onBatchCreated">event callback</param>
        /// <param name="onWaitingBatchUsable">event callback</param>
        /// <param name="onBatchUsable">event callback</param>
        Task<PostageBatchId> CreatePostageBatchAsync(
            BzzValue amount,
            int batchDepth,
            string? label,
            Action? onWaitingBatchCreation = null,
            Action<PostageBatchId>? onBatchCreated = null,
            Action? onWaitingBatchUsable = null,
            Action? onBatchUsable = null);

        /// <summary>
        /// Delete pin.
        /// </summary>
        /// <param name="reference">Resource hash</param>
        Task DefundResourcePinningAsync(SwarmReference reference);

        /// <summary>
        /// Offer the content to all users.
        /// </summary>
        /// <param name="hash">Resource hash</param>
        Task FundResourceDownloadAsync(SwarmHash hash);

        Task FundResourcePinningAsync(SwarmReference reference);
        
        Task<ChainState> GetChainStateAsync();

        Task<PostageBatch> GetPostageBatchInfoAsync(PostageBatchId batchId);

        Task<SwarmHash> UploadChunkAsync(
            PostageBatchId batchId,
            SwarmCac chunk,
            bool fundPinning = false,
            TagId? tagId = null);
        
        Task<SwarmReference> UploadDirectoryAsync(
            PostageBatchId batchId,
            string directoryPath,
            bool pinResource);
        
        Task<SwarmReference> UploadFileAsync(
            PostageBatchId batchId,
            Stream content,
            string? name,
            string? contentType,
            bool pinResource);
    }
}
