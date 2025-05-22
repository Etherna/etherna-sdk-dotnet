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
using Etherna.Sdk.Gateway.GenClients;
using Etherna.Sdk.Users.Gateway.Clients;
using Etherna.Sdk.Users.Gateway.Options;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Etherna.Sdk.Users.Gateway.Services
{
    public sealed class GatewayService(
        IEthernaUserGatewayClient ethernaGatewayClient,
        IOptions<GatewayServiceOptions> options)
        : IGatewayService
    {
        // Consts.
        public static readonly TimeSpan BatchCheckTimeSpan = new(0, 0, 0, 5);
        public static readonly TimeSpan BatchCreationTimeout = new(0, 0, 15, 0);
        public static readonly TimeSpan BatchUsableTimeout = new(0, 0, 15, 0);

        // Fields.
        private readonly GatewayServiceOptions options = options.Value;

        // Methods.
        public async Task ChunksBulkUploadAsync(
            SwarmChunk[] chunks,
            PostageBatchId batchId,
            bool swarmPin = false)
        {
            ArgumentNullException.ThrowIfNull(chunks, nameof(chunks));
            
            if (options.IsDryRun)
                return;
            if (options.UseBeeApi)
            {
                foreach (var chunk in chunks)
                {
                    using var memoryStream = new MemoryStream(chunk.GetFullPayloadToByteArray());
                    
                    await ethernaGatewayClient.UploadChunkAsync(
                        memoryStream,
                        batchId,
                        swarmPin).ConfigureAwait(false);
                }
                return;
            }

            await ethernaGatewayClient.ChunksBulkUploadAsync(chunks, batchId).ConfigureAwait(false);
        }

        public async Task<PostageBatchId> CreatePostageBatchAsync(
            BzzBalance amount,
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

            if (options.IsDryRun)
                return PostageBatchId.Zero;
            if (options.UseBeeApi)
            {
                // Create batch.
                onWaitingBatchCreation?.Invoke();
                var (batchId, _) = await ethernaGatewayClient.BeeClient.BuyPostageBatchAsync(
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

        public Task<TagInfo> CreateTagAsync(SwarmHash hash, PostageBatchId batchId)
        {
            if (options.IsDryRun)
                return Task.FromResult(new TagInfo(new TagId(0), DateTimeOffset.UtcNow, 0, 0, 0, 0, 0));
            return ethernaGatewayClient.BeeClient.CreateTagAsync(hash, batchId);
        }

        public Task DefundResourcePinningAsync(SwarmHash hash)
        {
            if (options.IsDryRun)
                return Task.CompletedTask;
            if (options.UseBeeApi)
                return ethernaGatewayClient.BeeClient.DeletePinAsync(hash);
            return ethernaGatewayClient.DefundResourcePinningAsync(hash);
        }

        public Task DeleteTagAsync(TagId tagId, PostageBatchId batchId)
        {
            if (options.IsDryRun)
                return Task.CompletedTask;
            return ethernaGatewayClient.BeeClient.DeleteTagAsync(tagId, batchId);
        }

        public Task FundResourceDownloadAsync(SwarmHash hash)
        {
            if (options.IsDryRun)
                return Task.CompletedTask;
            if (options.UseBeeApi)
                throw new NotSupportedException();
            return ethernaGatewayClient.FundResourceDownloadAsync(hash);
        }

        public Task FundResourcePinningAsync(SwarmHash hash)
        {
            if (options.IsDryRun)
                return Task.CompletedTask;
            if (options.UseBeeApi)
                return ethernaGatewayClient.BeeClient.CreatePinAsync(hash);
            return ethernaGatewayClient.FundResourcePinningAsync(hash);
        }

        public async Task<BzzBalance> GetChainPriceAsync()
        {
            if (options.UseBeeApi)
                return (await ethernaGatewayClient.BeeClient.GetChainStateAsync().ConfigureAwait(false)).CurrentPrice;
            return (await ethernaGatewayClient.GetChainStateAsync().ConfigureAwait(false)).CurrentPrice;
        }

        public Task<PostageBatch> GetPostageBatchInfoAsync(PostageBatchId batchId)
        {
            if (options.UseBeeApi)
                return ethernaGatewayClient.BeeClient.GetPostageBatchAsync(batchId);
            return ethernaGatewayClient.GetPostageBatchAsync(batchId);
        }

        public async Task<bool> IsBatchUsableAsync(PostageBatchId batchId)
        {
            if (options.UseBeeApi)
                return (await ethernaGatewayClient.BeeClient.GetPostageBatchAsync(batchId).ConfigureAwait(false)).IsUsable;
            return (await ethernaGatewayClient.GetPostageBatchAsync(batchId).ConfigureAwait(false)).IsUsable;
        }

        public Task UpdateTagInfoAsync(TagId tagId, SwarmHash rootHash, PostageBatchId batchId)
        {
            if (options.IsDryRun)
                return Task.CompletedTask;
            return ethernaGatewayClient.BeeClient.UpdateTagAsync(tagId, batchId, rootHash);
        }

        public async Task UploadChunkAsync(
            PostageBatchId batchId,
            SwarmCac chunk,
            bool fundPinning = false,
            TagId? tagId = null)
        {
            ArgumentNullException.ThrowIfNull(chunk, nameof(chunk));

            if (options.IsDryRun)
                return;
            
            using var dataStream = new MemoryStream(chunk.Data.ToArray());
            await ethernaGatewayClient.BeeClient.UploadChunkAsync(
                dataStream,
                batchId,
                pinChunk: fundPinning,
                tagId: tagId).ConfigureAwait(false);
        }
        
        public Task<SwarmHash> UploadDirectoryAsync(
            PostageBatchId batchId,
            string directoryPath,
            bool pinResource) =>
            ethernaGatewayClient.UploadDirectoryAsync(
                batchId,
                directoryPath,
                pinDirectory: pinResource);

        public Task<SwarmHash> UploadFileAsync(
            PostageBatchId batchId,
            Stream content,
            string? name,
            string? contentType,
            bool pinResource) =>
            ethernaGatewayClient.UploadFileAsync(
                batchId,
                content,
                name: name,
                contentType: contentType,
                pinFile: pinResource);

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
                    batchIsUsable = await IsBatchUsableAsync(batchId).ConfigureAwait(false);
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
