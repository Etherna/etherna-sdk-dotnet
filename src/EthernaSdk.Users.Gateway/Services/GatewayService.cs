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

using Etherna.Sdk.Gateway.GenClients;
using Etherna.Sdk.Users.Gateway.Clients;
using Etherna.Sdk.Users.Gateway.Extensions;
using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Models;
using Etherna.SwarmSdk.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RedundancyLevel = Etherna.SwarmSdk.Models.RedundancyLevel;
using RedundancyStrategy = Etherna.SwarmSdk.Models.RedundancyStrategy;
using SwarmFeedType = Etherna.SwarmSdk.Models.SwarmFeedType;

namespace Etherna.Sdk.Users.Gateway.Services
{
    public sealed class GatewayService(
        IEthernaUserGatewayClient ethernaGatewayClient,
        ISwarmClient swarmClient,
        TimeProvider? timeProvider = null,
        ILogger<GatewayService>? logger = null)
        : IGatewayService
    {
        // Consts.
        public static readonly TimeSpan BatchCheckTimeSpan = new(0, 0, 0, 5);
        public static readonly TimeSpan BatchCreationTimeout = new(0, 0, 15, 0);
        public static readonly TimeSpan BatchUsableTimeout = new(0, 0, 15, 0);

        // Fields.
        private readonly ILogger<GatewayService> logger = logger ?? NullLogger<GatewayService>.Instance;
        private readonly TimeProvider timeProvider = timeProvider ?? TimeProvider.System;

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

            // In dry mode no batch is bought, and there is none to wait for.
            if (swarmClient.IsDryMode)
            {
                logger.DryModeSkippedPostageBatchPurchase(depth, amount);

                onWaitingBatchCreation?.Invoke();
                onBatchCreated?.Invoke(PostageBatchId.Zero);
                onWaitingBatchUsable?.Invoke();
                onBatchUsable?.Invoke();

                return (PostageBatchId.Zero, EthTxHash.Zero);
            }

            // Create batch.
            PostageBatchId batchId;
            var txHash = EthTxHash.Zero;
            if (swarmClient.ApiCompatibility == SwarmClients.Bee)
            {
                onWaitingBatchCreation?.Invoke();
                (batchId, txHash) = await swarmClient.BuyPostageBatchAsync(
                    amount,
                    depth,
                    label,
                    immutable,
                    gasLimit,
                    gasPrice,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                //the gateway buys the batch with a background job, and answers with a reference to its result
                var batchReferenceId = await ethernaGatewayClient.BuyPostageBatchAsync(
                    amount,
                    depth,
                    label,
                    cancellationToken).ConfigureAwait(false);
                logger.PostageBatchPurchaseRequested(batchReferenceId, depth, amount);

                // Wait until created batch is available.
                onWaitingBatchCreation?.Invoke();

                var creationStartTimestamp = timeProvider.GetTimestamp();
                PostageBatchId? createdBatchId = null;
                while (true)
                {
                    // Until the gateway answers with the batch id, the batch is taken as not created yet, even
                    // when the gateway fails to answer. Only a 410 is final: with it the gateway reports the
                    // purchase it gave up, whose credit it gives back.
                    try
                    {
                        createdBatchId = await ethernaGatewayClient.TryGetNewPostageBatchIdFromPostageRefAsync(
                            batchReferenceId, cancellationToken).ConfigureAwait(false);
                        if (createdBatchId is null)
                            logger.PostageBatchNotCreatedYet(batchReferenceId);
                    }
                    catch (EthernaGatewayApiException e) when (e.StatusCode != 410)
                    {
                        logger.PostageBatchReferenceReadFailed(batchReferenceId, e.StatusCode, e);
                    }

                    if (createdBatchId is not null)
                        break;

                    if (timeProvider.GetElapsedTime(creationStartTimestamp) >= BatchCreationTimeout)
                        throw new TimeoutException(
                            $"Postage batch with reference {batchReferenceId} was not created within {BatchCreationTimeout}");

                    await Task.Delay(BatchCheckTimeSpan, timeProvider, cancellationToken).ConfigureAwait(false);
                }
                batchId = createdBatchId.Value;
            }
            logger.PostageBatchCreated(batchId, depth, amount);
            onBatchCreated?.Invoke(batchId);

            // Wait until created batch is usable.
            //the poller decides which failed reads to tolerate: each one is logged, whatever it decides
            onWaitingBatchUsable?.Invoke();
            await PostageBatchUsablePoller.WaitUntilUsableAsync(
                batchId,
                async (id, token) =>
                {
                    try
                    {
                        var batch = (await swarmClient.GetPostageBatchAsync(id, token).ConfigureAwait(false)).PostageBatch;
                        if (!batch.IsUsable)
                            logger.PostageBatchNotUsableYet(id);

                        return batch;
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        logger.PostageBatchReadFailed(id, e);
                        throw;
                    }
                },
                timeout: BatchUsableTimeout,
                pollInterval: BatchCheckTimeSpan,
                timeProvider: timeProvider,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            logger.PostageBatchUsable(batchId);
            onBatchUsable?.Invoke();

            return (batchId, txHash);
        }
        
        public Task ChunksBulkUploadAsync(
            SwarmChunk[] chunks,
            PostageBatchId batchId,
            CancellationToken cancellationToken = default) =>
            swarmClient.ChunksBulkUploadAsync(
                chunks,
                batchId,
                cancellationToken: cancellationToken);

        public Task<bool> CreatePinAsync(
            SwarmReference reference,
            RedundancyLevel? redundancyLevel = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.CreatePinAsync(reference, redundancyLevel, cancellationToken);

        public Task<bool> DefundResourceDownloadAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default)
        {
            if (swarmClient.ApiCompatibility == SwarmClients.Bee)
                throw new NotSupportedException();
            if (swarmClient.IsDryMode)
            {
                logger.DryModeSkippedResourceDownloadDefunding(reference);
                return Task.FromResult(false);
            }
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
            if (swarmClient.IsDryMode)
            {
                logger.DryModeSkippedResourceDownloadFunding(reference);
                return Task.CompletedTask;
            }
            return ethernaGatewayClient.FundResourceDownloadAsync(reference, cancellationToken);
        }

        public Task<SwarmReference[]> GetAllPinsAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetAllPinsAsync(cancellationToken);

        public Task<Stream> GetBytesAsync(
            SwarmReference reference,
            RedundancyLevel? redundancyLevel = null,
            RedundancyStrategy? redundancyStrategy = null,
            bool? redundancyFallbackMode = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.GetBytesAsync(
                reference: reference,
                redundancyLevel: redundancyLevel,
                redundancyStrategy: redundancyStrategy,
                redundancyFallbackMode: redundancyFallbackMode,
                cancellationToken: cancellationToken);

        public Task<SwarmResponseHeaders> GetBytesHeadersAsync(
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
            RedundancyLevel? redundancyLevel = null,
            RedundancyStrategy? redundancyStrategy = null,
            bool? redundancyFallbackMode = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.GetFileAsync(
                address,
                redundancyLevel: redundancyLevel,
                redundancyStrategy: redundancyStrategy,
                redundancyFallbackMode: redundancyFallbackMode,
                cancellationToken: cancellationToken);

        public Task<(PostageBatch PostageBatch, EthAddress Owner)[]> GetGlobalValidPostageBatchesAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetGlobalValidPostageBatchesAsync(cancellationToken: cancellationToken);

        public Task<Health> GetHealthAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetHealthAsync(cancellationToken);

        public Task<NodeInfo> GetNodeInfoAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetNodeInfoAsync(cancellationToken);

        public Task<PostageBatchResponse[]> GetOwnedPostageBatchesAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetOwnedPostageBatchesAsync(cancellationToken);

        public Task<PinStatus> GetPinStatusAsync(SwarmReference reference, CancellationToken cancellationToken = default) =>
            swarmClient.GetPinStatusAsync(reference, cancellationToken);

        public Task<PostageBatchResponse> GetPostageBatchAsync(PostageBatchId batchId, CancellationToken cancellationToken = default) =>
            swarmClient.GetPostageBatchAsync(batchId, cancellationToken);

        public Task<PostageBucketsStatus> GetPostageBatchBucketsAsync(PostageBatchId batchId, CancellationToken cancellationToken = default) =>
            swarmClient.GetPostageBatchBucketsAsync(batchId, cancellationToken);

        public Task<bool> GetReadinessAsync(CancellationToken cancellationToken = default) =>
            swarmClient.GetReadinessAsync(cancellationToken);

        public Task<FileResponse> GetSocDataAsync(
            EthAddress owner,
            string id,
            bool? onlyRootChunk = null,
            RedundancyStrategy? redundancyStrategy = null,
            bool? redundancyFallbackMode = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.GetSocDataAsync(
                owner: owner,
                id: id,
                onlyRootChunk: onlyRootChunk,
                redundancyStrategy: redundancyStrategy,
                redundancyFallbackMode: redundancyFallbackMode,
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
            bool? onlyRootChunk = null,
            RedundancyStrategy? redundancyStrategy = null,
            bool? redundancyFallbackMode = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.TryGetFeedAsync(
                owner: owner,
                topic: topic,
                at: at,
                after: after,
                afterLevel: afterLevel,
                type: type,
                onlyRootChunk: onlyRootChunk,
                redundancyStrategy: redundancyStrategy,
                redundancyFallbackMode: redundancyFallbackMode,
                cancellationToken: cancellationToken);

        public Task<SwarmResponseHeaders?> TryGetFileHeadersAsync(
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
            bool? pin = null,
            bool? encrypt = null,
            RedundancyLevel redundancyLevel = RedundancyLevel.None,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadBytesAsync(
                body,
                batchId: batchId,
                compactLevel: compactLevel,
                pin: pin,
                encrypt: encrypt,
                redundancyLevel: redundancyLevel,
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
            bool? pin = null,
            bool? encrypt = null,
            string? indexDocument = null,
            string? errorDocument = null,
            RedundancyLevel redundancyLevel = RedundancyLevel.None,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadDirectoryAsync(
                directoryPath,
                batchId: batchId,
                compactLevel: compactLevel,
                pin: pin,
                encrypt: encrypt,
                indexDocument: indexDocument,
                errorDocument: errorDocument,
                redundancyLevel: redundancyLevel,
                cancellationToken: cancellationToken);

        public Task<SwarmHash> UploadFeedManifestAsync(
            SwarmFeedBase feed,
            PostageBatchId batchId,
            ushort? compactLevel,
            bool pin = false,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadFeedManifestAsync(
                feed,
                batchId: batchId,
                compactLevel: compactLevel,
                pin: pin,
                cancellationToken: cancellationToken);

        public Task<SwarmReference> UploadFileAsync(
            Stream content,
            PostageBatchId batchId,
            ushort? compactLevel,
            string? name = null,
            string? contentType = null,
            bool isFileCollection = false,
            bool? pin = null,
            bool? encrypt = null,
            string? indexDocument = null,
            string? errorDocument = null,
            RedundancyLevel redundancyLevel = RedundancyLevel.None,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadFileAsync(
                content,
                batchId: batchId,
                compactLevel: compactLevel,
                name: name,
                contentType: contentType,
                isFileCollection: isFileCollection,
                pin: pin,
                encrypt: encrypt,
                indexDocument: indexDocument,
                errorDocument: errorDocument,
                redundancyLevel: redundancyLevel,
                cancellationToken: cancellationToken);

        public Task<SwarmHash> UploadSocAsync(
            SwarmSoc soc,
            PostageBatchId? batchId,
            PostageStamp? presignedPostageStamp = null,
            bool? pin = null,
            CancellationToken cancellationToken = default) =>
            swarmClient.UploadSocAsync(
                soc,
                batchId: batchId,
                presignedPostageStamp: presignedPostageStamp,
                pin: pin,
                cancellationToken: cancellationToken);
    }
}
