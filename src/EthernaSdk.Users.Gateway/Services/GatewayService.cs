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
using System.Net.Http.Headers;
using System.Threading;
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
        public async Task<(PostageBatchId BatchId, EthTxHash TxHash)> BuyPostageBatchAsync(
            BzzValue amount,
            int depth,
            string? label = null,
            bool? immutable = null,
            ulong? gasLimit = null,
            XDaiValue? gasPrice = null,
            Action? onWaitingBatchCreation = null,
            Action<PostageBatchId>? onBatchCreated = null,
            Action? onWaitingBatchUsable = null,
            Action? onBatchUsable = null,
            CancellationToken cancellationToken = default)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");
            if (depth < PostageBatch.MinDepth)
                throw new ArgumentException($"Postage depth must be at least {PostageBatch.MinDepth}");

            if (swarmClient.ApiCompatibility == SwarmClients.Bee)
            {
                // Create batch.
                onWaitingBatchCreation?.Invoke();
                var (batchId, txHash) = await swarmClient.BuyPostageBatchAsync(
                    amount,
                    depth,
                    label,
                    immutable,
                    gasLimit,
                    gasPrice,
                    cancellationToken).ConfigureAwait(false);
                onBatchCreated?.Invoke(batchId);

                // Wait until created batch is usable.
                onWaitingBatchUsable?.Invoke();
                await WaitForBatchUsableAsync(batchId).ConfigureAwait(false);
                onBatchUsable?.Invoke();

                return (batchId, txHash);
            }
            else
            {
                var batchReferenceId = await ethernaGatewayClient.BuyPostageBatchAsync(
                    amount,
                    depth,
                    label,
                    cancellationToken).ConfigureAwait(false);

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
                        batchId = await ethernaGatewayClient.TryGetNewPostageBatchIdFromPostageRefAsync(
                            batchReferenceId, cancellationToken).ConfigureAwait(false);
                    }
                    catch (EthernaGatewayApiException)
                    {
                        //waiting for batchId available
                        await Task.Delay(BatchCheckTimeSpan, cancellationToken).ConfigureAwait(false);
                    }
                } while (batchId is null);

                onBatchCreated?.Invoke(batchId.Value);

                // Wait until created batch is usable.
                onWaitingBatchUsable?.Invoke();
                await WaitForBatchUsableAsync(batchId.Value).ConfigureAwait(false);
                onBatchUsable?.Invoke();

                return (batchId.Value, EthTxHash.Zero);
            }
        }
        
        public Task ChunksBulkUploadAsync(
            SwarmChunk[] chunks,
            PostageBatchId batchId,
            CancellationToken cancellationToken = default) =>
            swarmClient.ChunksBulkUploadAsync(chunks, batchId, cancellationToken);

        public Task<bool> CreatePinAsync(SwarmReference reference, CancellationToken cancellationToken = default) =>
            swarmClient.CreatePinAsync(reference, cancellationToken);

        public Task<bool> DefundResourceDownloadAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default)
        {
            if (swarmClient.ApiCompatibility == SwarmClients.Bee)
                throw new NotSupportedException();
            return ethernaGatewayClient.DefundResourceDownloadAsync(reference, cancellationToken);
        }

        public Task DeletePinAsync(SwarmReference reference, CancellationToken cancellationToken = default) =>
            swarmClient.DeletePinAsync(reference, cancellationToken);

        public Task<EthTxHash> DilutePostageBatchAsync(
            PostageBatchId batchId,
            int depth,
            XDaiValue? gasPrice = null,
            ulong? gasLimit = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.DilutePostageBatchAsync(batchId, depth, gasPrice, gasLimit, cancellationToken);

        public Task DefundResourcePinningAsync(SwarmReference reference) =>
            swarmClient.DeletePinAsync(reference);

        public Task FundResourceDownloadAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default)
        {
            if (swarmClient.ApiCompatibility == SwarmClients.Bee)
                throw new NotSupportedException();
            return ethernaGatewayClient.FundResourceDownloadAsync(reference, cancellationToken);
        }

        public Task<SwarmReference[]> GetAllPinsAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetAllPinsAsync(cancellationToken);

        public Task<Stream> GetBytesAsync(
            SwarmReference reference,
            RedundancyLevel? swarmRedundancyLevel = null,
            RedundancyStrategy? swarmRedundancyStrategy = null,
            bool? swarmRedundancyFallbackMode = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.GetBytesAsync(
                reference,
                swarmRedundancyLevel: swarmRedundancyLevel,
                swarmRedundancyStrategy: swarmRedundancyStrategy,
                swarmRedundancyFallbackMode: swarmRedundancyFallbackMode,
                cancellationToken: cancellationToken);

        public Task<HttpContentHeaders?> GetBytesHeadersAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default) =>
            swarmClient.GetBytesHeadersAsync(reference, cancellationToken: cancellationToken);

        public Task<ChainState> GetChainStateAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetChainStateAsync(cancellationToken);

        public Task<SwarmChunk> GetChunkAsync(
            SwarmHash hash,
            SwarmChunkBmt swarmChunkBmt,
            CancellationToken cancellationToken = default) =>
            swarmClient.GetChunkAsync(hash, swarmChunkBmt, cancellationToken: cancellationToken);

        public Task<Stream> GetChunkStreamAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            swarmClient.GetChunkStreamAsync(hash, cancellationToken: cancellationToken);

        public Task<FileResponse> GetFileAsync(
            SwarmAddress address,
            RedundancyLevel? swarmRedundancyLevel = null,
            RedundancyStrategy? swarmRedundancyStrategy = null,
            bool? swarmRedundancyFallbackMode = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.GetFileAsync(
                address,
                swarmRedundancyLevel: swarmRedundancyLevel,
                swarmRedundancyStrategy: swarmRedundancyStrategy,
                swarmRedundancyFallbackMode: swarmRedundancyFallbackMode,
                cancellationToken: cancellationToken);

        public Task<(PostageBatch PostageBatch, EthAddress Owner)[]> GetGlobalValidPostageBatchesAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetGlobalValidPostageBatchesAsync(cancellationToken);

        public Task<Health> GetHealthAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetHealthAsync(cancellationToken);

        public Task<NodeInfo> GetNodeInfoAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetNodeInfoAsync(cancellationToken);

        public Task<PostageBatch[]> GetOwnedPostageBatchesAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetOwnedPostageBatchesAsync(cancellationToken);

        public Task<bool> GetPinStatusAsync(SwarmReference reference, CancellationToken cancellationToken = default) =>
            swarmClient.GetPinStatusAsync(reference, cancellationToken);

        public Task<PostageBatch> GetPostageBatchAsync(PostageBatchId batchId, CancellationToken cancellationToken = default) =>
            swarmClient.GetPostageBatchAsync(batchId, cancellationToken);

        public Task<PostageBucketsStatus> GetPostageBatchBucketsAsync(PostageBatchId batchId, CancellationToken cancellationToken = default) =>
            swarmClient.GetPostageBatchBucketsAsync(batchId, cancellationToken);

        public Task<bool> GetReadinessAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetReadinessAsync(cancellationToken);

        public Task<FileResponse> GetSocDataAsync(
            EthAddress owner,
            string id,
            bool? swarmOnlyRootChunk = null,
            RedundancyStrategy? swarmRedundancyStrategy = null,
            bool? swarmRedundancyFallbackMode = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.GetSocDataAsync(
                owner: owner,
                id: id,
                swarmOnlyRootChunk: swarmOnlyRootChunk,
                swarmRedundancyStrategy: swarmRedundancyStrategy,
                swarmRedundancyFallbackMode: swarmRedundancyFallbackMode,
                cancellationToken: cancellationToken);

        public Task<bool> IsChunkExistingAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            swarmClient.IsChunkExistingAsync(hash, cancellationToken: cancellationToken);

        public Task<EthTxHash> TopUpPostageBatchAsync(
            PostageBatchId batchId,
            BzzValue amount,
            XDaiValue? gasPrice = null,
            ulong? gasLimit = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.TopUpPostageBatchAsync(batchId, amount, gasPrice, gasLimit, cancellationToken);

        public Task<FileResponse?> TryGetFeedAsync(
            EthAddress owner,
            SwarmFeedTopic topic,
            long? at = null,
            ulong? after = null,
            int? afterLevel = null,
            SwarmFeedType type = SwarmFeedType.Sequence,
            bool? swarmOnlyRootChunk = null,
            RedundancyStrategy? swarmRedundancyStrategy = null,
            bool? swarmRedundancyFallbackMode = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.TryGetFeedAsync(
                owner: owner,
                topic: topic,
                at: at,
                after: after,
                afterLevel: afterLevel,
                type: type,
                swarmOnlyRootChunk: swarmOnlyRootChunk,
                swarmRedundancyStrategy: swarmRedundancyStrategy,
                swarmRedundancyFallbackMode: swarmRedundancyFallbackMode,
                cancellationToken: cancellationToken);

        public Task<HttpContentHeaders?> TryGetFileHeadersAsync(
            SwarmAddress address,
            RedundancyLevel? redundancyLevel = null,
            RedundancyStrategy? redundancyStrategy = null,
            bool? redundancyStrategyFallback = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.TryGetFileHeadersAsync(
                address,
                redundancyLevel: redundancyLevel,
                redundancyStrategy: redundancyStrategy,
                redundancyStrategyFallback: redundancyStrategyFallback,
                cancellationToken: cancellationToken);

        public Task<long?> TryGetFileSizeAsync(
            SwarmAddress address,
            RedundancyLevel? redundancyLevel = null,
            RedundancyStrategy? redundancyStrategy = null,
            bool? redundancyStrategyFallback = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.TryGetFileSizeAsync(
                address,
                redundancyLevel: redundancyLevel,
                redundancyStrategy: redundancyStrategy,
                redundancyStrategyFallback: redundancyStrategyFallback,
                cancellationToken: cancellationToken);

        public Task<SwarmReference> UploadBytesAsync(
            Stream body,
            PostageBatchId batchId,
            ushort? compactLevel,
            bool? swarmPin = null,
            bool? swarmEncrypt = null,
            RedundancyLevel swarmRedundancyLevel = RedundancyLevel.None,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadBytesAsync(
                body,
                batchId: batchId,
                compactLevel: compactLevel,
                swarmPin: swarmPin,
                swarmEncrypt: swarmEncrypt,
                swarmRedundancyLevel: swarmRedundancyLevel,
                cancellationToken: cancellationToken);

        public Task<SwarmHash> UploadChunkAsync(
            Stream chunkData,
            PostageBatchId? batchId,
            PostageStamp? presignedPostageStamp = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadChunkAsync(
                chunkData,
                batchId: batchId,
                presignedPostageStamp: presignedPostageStamp,
                cancellationToken: cancellationToken);

        public Task<SwarmHash> UploadChunkAsync(
            SwarmCac chunk,
            PostageBatchId? batchId,
            PostageStamp? presignedPostageStamp = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadChunkAsync(
                chunk,
                batchId: batchId,
                presignedPostageStamp: presignedPostageStamp,
                cancellationToken: cancellationToken);

        public Task<SwarmReference> UploadDirectoryAsync(
            string directoryPath,
            PostageBatchId batchId,
            ushort? compactLevel,
            bool? swarmPin = null,
            bool? swarmEncrypt = null,
            string? swarmIndexDocument = null,
            string? swarmErrorDocument = null,
            RedundancyLevel swarmRedundancyLevel = RedundancyLevel.None,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadDirectoryAsync(
                directoryPath,
                batchId: batchId,
                compactLevel: compactLevel,
                swarmPin: swarmPin,
                swarmEncrypt: swarmEncrypt,
                swarmIndexDocument: swarmIndexDocument,
                swarmErrorDocument: swarmErrorDocument,
                swarmRedundancyLevel: swarmRedundancyLevel,
                cancellationToken: cancellationToken);

        public Task<SwarmHash> UploadFeedManifestAsync(
            SwarmFeedBase feed,
            PostageBatchId batchId,
            ushort? compactLevel,
            bool swarmPin = false,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadFeedManifestAsync(
                feed,
                batchId: batchId,
                compactLevel: compactLevel,
                swarmPin: swarmPin,
                cancellationToken: cancellationToken);

        public Task<SwarmReference> UploadFileAsync(
            Stream content,
            PostageBatchId batchId,
            ushort? compactLevel,
            string? name = null,
            string? contentType = null,
            bool isFileCollection = false,
            bool? swarmPin = null,
            bool? swarmEncrypt = null,
            string? swarmIndexDocument = null,
            string? swarmErrorDocument = null,
            RedundancyLevel swarmRedundancyLevel = RedundancyLevel.None,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadFileAsync(
                content,
                batchId: batchId,
                compactLevel: compactLevel,
                name: name,
                contentType: contentType,
                isFileCollection: isFileCollection,
                swarmPin: swarmPin,
                swarmEncrypt: swarmEncrypt,
                swarmIndexDocument: swarmIndexDocument,
                swarmErrorDocument: swarmErrorDocument,
                swarmRedundancyLevel: swarmRedundancyLevel,
                cancellationToken: cancellationToken);

        public Task<SwarmHash> UploadSocAsync(
            SwarmSoc soc,
            PostageBatchId? batchId,
            PostageStamp? presignedPostageStamp = null,
            bool? swarmPin = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadSocAsync(
                soc,
                batchId: batchId,
                presignedPostageStamp: presignedPostageStamp,
                swarmPin: swarmPin,
                cancellationToken: cancellationToken);

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
