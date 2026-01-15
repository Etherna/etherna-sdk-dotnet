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

using Etherna.BeeNet;
using Etherna.BeeNet.Models;
using Etherna.Sdk.Gateway.GenClients;
using Etherna.Sdk.Users.Gateway.Clients;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Etherna.Sdk.Users.Gateway.Services
{
    public sealed class GatewayService(
        IEthernaUserGatewayClient ethernaGatewayClient,
        ISwarmClient swarmClient)
        : IGatewayService
    {
        // Consts.
        public static readonly TimeSpan BatchCheckTimeSpan = new(0, 0, 0, 5);
        public static readonly TimeSpan BatchCreationTimeout = new(0, 0, 15, 0);
        public static readonly TimeSpan BatchUsableTimeout = new(0, 0, 15, 0);

        // Methods.
        public Task ChunksBulkUploadAsync(
            SwarmChunk[] chunks,
            PostageBatchId batchId) =>
            swarmClient.ChunksBulkUploadAsync(chunks, batchId);

        public async Task<PostageBatchId> CreatePostageBatchAsync(
            BzzValue amount,
            int batchDepth,
            string? label,
            Action? onWaitingBatchCreation = null,
            Action<PostageBatchId>? onBatchCreated = null,
            Action? onWaitingBatchUsable = null,
            Action? onBatchUsable = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");
            if (batchDepth < PostageBatch.MinDepth)
                throw new ArgumentException($"Postage depth must be at least {PostageBatch.MinDepth}");

            if (swarmClient.ApiCompatibility == SwarmClients.Bee)
            {
                // Create batch.
                onWaitingBatchCreation?.Invoke();
                var (batchId, _) = await swarmClient.BuyPostageBatchAsync(
                    amount,
                    batchDepth,
                    label).ConfigureAwait(false);
                onBatchCreated?.Invoke(batchId);

                // Wait until created batch is usable.
                onWaitingBatchUsable?.Invoke();
                await WaitForBatchUsableAsync(batchId).ConfigureAwait(false);
                onBatchUsable?.Invoke();

                return batchId;
            }
            else
            {
                var batchReferenceId = await ethernaGatewayClient.BuyPostageBatchAsync(
                    amount,
                    batchDepth,
                    label).ConfigureAwait(false);

                // Wait until created batch is available.
                onWaitingBatchCreation?.Invoke();

                var batchStartWait = DateTime.UtcNow;
                PostageBatchId? batchId = null;
                do
                {
                    //timeout throw exception
                    if (DateTime.UtcNow - batchStartWait >= BatchCreationTimeout)
                    {
                        var ex = new InvalidOperationException("Batch not available after timeout");
                        ex.Data.Add("BatchReferenceId", batchReferenceId);
                        throw ex;
                    }

                    try
                    {
                        batchId = await ethernaGatewayClient.TryGetNewPostageBatchIdFromPostageRefAsync(batchReferenceId).ConfigureAwait(false);
                    }
                    catch (EthernaGatewayApiException)
                    {
                        //waiting for batchId available
                        await Task.Delay(BatchCheckTimeSpan).ConfigureAwait(false);
                    }
                } while (batchId is null);

                onBatchCreated?.Invoke(batchId.Value);

                // Wait until created batch is usable.
                onWaitingBatchUsable?.Invoke();
                await WaitForBatchUsableAsync(batchId.Value).ConfigureAwait(false);
                onBatchUsable?.Invoke();

                return batchId.Value;
            }
        }

        public Task DefundResourcePinningAsync(SwarmReference reference) =>
            swarmClient.DeletePinAsync(reference);

        public Task FundResourceDownloadAsync(SwarmHash hash)
        {
            if (swarmClient.ApiCompatibility == SwarmClients.Bee)
                throw new NotSupportedException();
            return ethernaGatewayClient.FundResourceDownloadAsync(hash);
        }

        public Task FundResourcePinningAsync(SwarmReference reference) =>
            swarmClient.CreatePinAsync(reference);

        public Task<ChainState> GetChainStateAsync() =>
            swarmClient.GetChainStateAsync();

        public Task<PostageBatch> GetPostageBatchInfoAsync(PostageBatchId batchId) =>
            swarmClient.GetPostageBatchAsync(batchId);

        public Task<SwarmHash> UploadChunkAsync(
            PostageBatchId batchId,
            SwarmCac chunk,
            bool fundPinning = false,
            TagId? tagId = null) =>
            swarmClient.UploadChunkAsync(
                chunk,
                batchId,
                pinChunk: fundPinning,
                tagId: tagId);
        
        public Task<SwarmReference> UploadDirectoryAsync(
            PostageBatchId batchId,
            string directoryPath,
            bool pinResource) =>
            swarmClient.UploadDirectoryAsync(
                directoryPath,
                batchId,
                swarmPin: pinResource);

        public Task<SwarmReference> UploadFileAsync(
            PostageBatchId batchId,
            Stream content,
            string? name,
            string? contentType,
            bool pinResource) =>
            swarmClient.UploadFileAsync(
                content,
                batchId,
                name: name,
                contentType: contentType,
                swarmPin: pinResource);

        // Helpers.
        private async Task WaitForBatchUsableAsync(PostageBatchId batchId)
        {
            var batchStartWait = DateTime.UtcNow;
            var batchIsUsable = false;
            do
            {
                //timeout throw exception
                if (DateTime.UtcNow - batchStartWait >= BatchUsableTimeout)
                {
                    var ex = new InvalidOperationException("Batch not usable after timeout");
                    ex.Data.Add("BatchId", batchId);
                    throw ex;
                }

                try
                {
                    var batchInfo = await swarmClient.GetPostageBatchAsync(batchId).ConfigureAwait(false);
                    batchIsUsable = batchInfo.IsUsable;
                }
                catch (EthernaGatewayApiException e) when
                    (e.StatusCode == 504) //single request timeout
                { }

                //waiting for batch usable
                if (!batchIsUsable) 
                    await Task.Delay(BatchCheckTimeSpan).ConfigureAwait(false);
            } while (!batchIsUsable);
        }
    }
}
