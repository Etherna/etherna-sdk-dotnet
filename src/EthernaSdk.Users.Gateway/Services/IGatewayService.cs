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

using Etherna.BeeNet.Exceptions;
using Etherna.BeeNet.Models;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.Users.Gateway.Services
{
    public interface IGatewayService
    {
        // Methods.
        /// <summary>Buy a new postage batch.</summary>
        /// <param name="amount">Amount of BZZ added that the postage batch will have.</param>
        /// <param name="depth">Batch depth which specifies how many chunks can be signed with the batch. It is a logarithm. Must be higher than default bucket depth (16)</param>
        /// <param name="label">An optional label for this batch</param>
        /// <param name="gasPrice">Gas price for transaction</param>
        /// <returns>Returns the newly created postage batch ID</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<(PostageBatchId BatchId, EthTxHash TxHash)> BuyPostageBatchAsync(
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
            CancellationToken cancellationToken = default);

        Task ChunksBulkUploadAsync(
            SwarmChunk[] chunks,
            PostageBatchId batchId,
            CancellationToken cancellationToken = default);

        /// <summary>Pin the root hash with the given reference</summary>
        /// <param name="reference">Swarm content reference</param>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<bool> CreatePinAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default);

        Task<bool> DefundResourceDownloadAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default);

        /// <summary>Unpin the root hash with the given reference</summary>
        /// <param name="reference">Swarm content reference</param>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task DeletePinAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default);

        /// <summary>Dilute an existing postage batch.</summary>
        /// <param name="batchId">Batch ID to dilute</param>
        /// <param name="depth">New batch depth. Must be higher than the previous depth.</param>
        /// <returns>Returns the tx hash updating the postage batch</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<EthTxHash> DilutePostageBatchAsync(
            PostageBatchId batchId,
            int depth,
            XDaiValue? gasPrice = null,
            ulong? gasLimit = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Offer the content to all users.
        /// </summary>
        /// <param name="hash">Resource hash</param>
        Task FundResourceDownloadAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default);

        /// <summary>Get the list of pinned root hash references</summary>
        /// <returns>List of pinned references</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<SwarmReference[]> GetAllPinsAsync(CancellationToken cancellationToken = default);

        /// <summary>Get referenced data</summary>
        /// <param name="reference">Swarm content reference</param>
        /// <param name="swarmRedundancyLevel"></param>
        /// <param name="swarmRedundancyStrategy">Specify the retrieve strategy on redundant data. The numbers stand for NONE, DATA, PROX and RACE, respectively. Strategy NONE means no prefetching takes place. Strategy DATA means only data chunks are prefetched. Strategy PROX means only chunks that are close to the node are prefetched. Strategy RACE means all chunks are prefetched: n data chunks and k parity chunks. The first n chunks to arrive are used to reconstruct the file. Multiple strategies can be used in a fallback cascade if the swarm redundancy fallback mode is set to true. The default strategy is NONE, DATA, falling back to PROX, falling back to RACE</param>
        /// <param name="swarmRedundancyFallbackMode">Specify if the retrieve strategies (chunk prefetching on redundant data) are used in a fallback cascade. The default is true.</param>
        /// <returns>Retrieved content specified by reference</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<Stream> GetBytesAsync(
            SwarmReference reference,
            RedundancyLevel? swarmRedundancyLevel = null,
            RedundancyStrategy? swarmRedundancyStrategy = null,
            bool? swarmRedundancyFallbackMode = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Requests the headers containing the content type and length for the reference
        /// </summary>
        /// <param name="reference">Swarm content reference</param>
        /// <returns>Chunk exists</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<HttpContentHeaders?> GetBytesHeadersAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default);

        /// <summary>Get chain state</summary>
        /// <returns>Chain State</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<ChainState> GetChainStateAsync(CancellationToken cancellationToken = default);

        /// <summary>Get Chunk</summary>
        /// <returns>Retrieved chunk content</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<SwarmChunk> GetChunkAsync(
            SwarmHash hash,
            SwarmChunkBmt swarmChunkBmt,
            CancellationToken cancellationToken = default);

        /// <summary>Get Chunk</summary>
        /// <returns>Retrieved chunk content</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<Stream> GetChunkStreamAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <summary>Get file or index document from a collection of files</summary>
        /// <param name="address">Swarm address of content</param>
        /// <param name="swarmCache">Determines if the download data should be cached on the node. By default the download will be cached</param>
        /// <param name="swarmRedundancyStrategy">Specify the retrieve strategy on redundant data. The numbers stand for NONE, DATA, PROX and RACE, respectively. Strategy NONE means no prefetching takes place. Strategy DATA means only data chunks are prefetched. Strategy PROX means only chunks that are close to the node are prefetched. Strategy RACE means all chunks are prefetched: n data chunks and k parity chunks. The first n chunks to arrive are used to reconstruct the file. Multiple strategies can be used in a fallback cascade if the swarm redundancy fallback mode is set to true. The default strategy is NONE, DATA, falling back to PROX, falling back to RACE</param>
        /// <param name="swarmRedundancyFallbackMode">Specify if the retrieve strategies (chunk prefetching on redundant data) are used in a fallback cascade. The default is true.</param>
        /// <param name="swarmChunkRetrievalTimeout">Specify the timeout for chunk retrieval. The default is 30 seconds.</param>
        /// <returns>Ok</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<FileResponse> GetFileAsync(
            SwarmAddress address,
            RedundancyLevel? swarmRedundancyLevel = null,
            RedundancyStrategy? swarmRedundancyStrategy = null,
            bool? swarmRedundancyFallbackMode = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all globally available batches that were purchased by all nodes.
        /// </summary>
        /// <returns></returns>
        /// <returns>Returns a dictionary with owner as keys, and enumerable of currently valid owned postage batches as values.</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<(PostageBatch PostageBatch, EthAddress Owner)[]> GetGlobalValidPostageBatchesAsync(CancellationToken cancellationToken = default);

        /// <summary>Get health of node</summary>
        /// <returns>Health State of node</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<Health> GetHealthAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get information about the node
        /// </summary>
        /// <returns>Information about the node</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<NodeInfo> GetNodeInfoAsync(CancellationToken cancellationToken = default);

        /// <summary>Get all owned postage batches by this node</summary>
        /// <returns>List of all owned postage batches</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred</exception>
        Task<PostageBatch[]> GetOwnedPostageBatchesAsync(CancellationToken cancellationToken = default);

        /// <summary>Get pinning status of the root hash with the given reference</summary>
        /// <param name="reference">Swarm content reference</param>
        /// <returns>Reference of the pinned root hash</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<bool> GetPinStatusAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default);

        /// <summary>Get an individual postage batch status</summary>
        /// <param name="batchId">Swarm address of the stamp</param>
        /// <returns>Returns an individual postage batch state</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<PostageBatch> GetPostageBatchAsync(
            PostageBatchId batchId,
            CancellationToken cancellationToken = default);

        /// <summary>Get extended bucket data of a batch</summary>
        /// <param name="batchId">Swarm address of the stamp</param>
        /// <returns>Returns extended bucket data of the provided batch ID</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<PostageBucketsStatus> GetPostageBatchBucketsAsync(
            PostageBatchId batchId,
            CancellationToken cancellationToken = default);

        Task<bool> GetReadinessAsync(CancellationToken cancellationToken = default);

        Task<FileResponse> GetSocDataAsync(
            EthAddress owner,
            string id,
            bool? swarmOnlyRootChunk = null,
            RedundancyStrategy? swarmRedundancyStrategy = null,
            bool? swarmRedundancyFallbackMode = null,
            CancellationToken cancellationToken = default);
        
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Check if chunk at address exists locally
        /// </summary>
        /// <param name="hash">Swarm hash of chunk</param>
        /// <returns>Chunk exists</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<bool> IsChunkExistingAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <summary>Top up an existing postage batch.</summary>
        /// <param name="batchId">Batch ID to top up</param>
        /// <param name="amount">Amount of BZZ per chunk to top up to an existing postage batch.</param>
        /// <returns>Returns the tx hash updating the postage batch</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<EthTxHash> TopUpPostageBatchAsync(
            PostageBatchId batchId, 
            BzzValue amount,
            XDaiValue? gasPrice = null,
            ulong? gasLimit = null,
            CancellationToken cancellationToken = default);

        /// <summary>Find feed update</summary>
        /// <param name="owner">Owner</param>
        /// <param name="topic">Topic</param>
        /// <param name="at">Timestamp of the update (default: now)</param>
        /// <param name="after">Start index (default: 0)</param>
        /// <param name="type">Feed indexing scheme (default: sequence)</param>
        /// <returns>Latest feed update</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<FileResponse?> TryGetFeedAsync(
            EthAddress owner,
            SwarmFeedTopic topic,
            long? at = null,
            ulong? after = null,
            int? afterLevel = null,
            SwarmFeedType type = SwarmFeedType.Sequence,
            bool? swarmOnlyRootChunk = null,
            RedundancyStrategy? swarmRedundancyStrategy = null,
            bool? swarmRedundancyFallbackMode = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Try to get file headers
        /// </summary>
        /// <param name="address">Swarm address of chunk</param>
        /// <returns>Headers with values</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<HttpContentHeaders?> TryGetFileHeadersAsync(
            SwarmAddress address,
            RedundancyLevel? redundancyLevel = null,
            RedundancyStrategy? redundancyStrategy = null, 
            bool? redundancyStrategyFallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Try to get the byte length of a file
        /// </summary>
        /// <param name="address">Swarm address of chunk</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Byte size of file</returns>
        Task<long?> TryGetFileSizeAsync(
            SwarmAddress address,
            RedundancyLevel? redundancyLevel = null,
            RedundancyStrategy? redundancyStrategy = null,
            bool? redundancyStrategyFallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>Upload data</summary>
        /// <param name="body"></param>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="tagId">Associate upload with an existing Tag UID</param>
        /// <param name="swarmPin">Represents if the uploaded data should be also locally pinned on the node.</param>
        /// <param name="swarmEncrypt">Represents the encrypting state of the file</param>
        /// <param name="swarmDeferredUpload">Determines if the uploaded data should be sent to the network immediately or in a deferred fashion. By default the upload will be deferred.</param>
        /// <param name="swarmRedundancyLevel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Content reference</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<SwarmReference> UploadBytesAsync(
            Stream body,
            PostageBatchId batchId,
            ushort? compactLevel = 0,
            bool? swarmPin = null,
            bool? swarmEncrypt = null,
            RedundancyLevel swarmRedundancyLevel = RedundancyLevel.None,
            CancellationToken cancellationToken = default);

        /// <summary>Upload Chunk</summary>
        /// <param name="chunkData"></param>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="pinChunk">Represents if the uploaded data should be also locally pinned on the node.</param>
        /// <param name="tagId">Associate upload with an existing Tag UID</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Content reference</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<SwarmHash> UploadChunkAsync(
            Stream chunkData,
            PostageBatchId? batchId,
            PostageStamp? presignedPostageStamp = null,
            CancellationToken cancellationToken = default);

        /// <summary>Upload Chunk</summary>
        /// <param name="chunk"></param>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="pinChunk">Represents if the uploaded data should be also locally pinned on the node.</param>
        /// <param name="tagId">Associate upload with an existing Tag UID</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Content reference</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<SwarmHash> UploadChunkAsync(
            SwarmCac chunk,
            PostageBatchId? batchId,
            PostageStamp? presignedPostageStamp = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>Upload a directory</summary>
        /// <param name="directoryPath">The directory path</param>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="tagId">Associate upload with an existing Tag UID</param>
        /// <param name="swarmPin">Represents if the uploaded data should be also locally pinned on the node.</param>
        /// <param name="swarmEncrypt">Represents the encrypting state of the file</param>
        /// <param name="swarmIndexDocument">Default file to be referenced on path, if exists under that path</param>
        /// <param name="swarmErrorDocument">Configure custom error document to be returned when a specified path can not be found in collection</param>
        /// <param name="swarmDeferredUpload">Determines if the uploaded data should be sent to the network immediately or in a deferred fashion. By default the upload will be deferred.</param>
        /// <param name="swarmRedundancyLevel">Add redundancy to the data being uploaded so that downloaders can download it with better UX. 0 value is default and does not add any redundancy to the file.</param>
        /// <param name="swarmAct"></param>
        /// <param name="swarmActHistoryAddress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Content reference</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<SwarmReference> UploadDirectoryAsync(
            string directoryPath,
            PostageBatchId batchId,
            ushort? compactLevel = 0,
            bool? swarmPin = null,
            bool? swarmEncrypt = null,
            string? swarmIndexDocument = null,
            string? swarmErrorDocument = null,
            RedundancyLevel swarmRedundancyLevel = RedundancyLevel.None,
            CancellationToken cancellationToken = default);

        /// <summary>Upload feed root manifest</summary>
        /// <param name="feed">Feed</param>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="swarmPin">Represents if the uploaded data should be also locally pinned on the node.</param>
        /// <returns>Reference hash</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<SwarmHash> UploadFeedManifestAsync(
            SwarmFeedBase feed,
            PostageBatchId batchId,
            ushort? compactLevel = 0,
            bool swarmPin = false,
            CancellationToken cancellationToken = default);

        /// <summary>Upload a file</summary>
        /// <param name="content">Input file content</param>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="name">Filename when uploading single file</param>
        /// <param name="contentType">The specified content-type is preserved for download of the asset</param>
        /// <param name="isFileCollection">Upload file/files as a collection</param>
        /// <param name="tagId">Associate upload with an existing Tag UID</param>
        /// <param name="swarmPin">Represents if the uploaded data should be also locally pinned on the node.</param>
        /// <param name="swarmEncrypt">Represents the encrypting state of the file</param>
        /// <param name="swarmIndexDocument">Default file to be referenced on path, if exists under that path</param>
        /// <param name="swarmErrorDocument">Configure custom error document to be returned when a specified path can not be found in collection</param>
        /// <param name="swarmDeferredUpload">Determines if the uploaded data should be sent to the network immediately or in a deferred fashion. By default the upload will be deferred.</param>
        /// <param name="swarmRedundancyLevel">Add redundancy to the data being uploaded so that downloaders can download it with better UX. 0 value is default and does not add any redundancy to the file.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Content reference</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<SwarmReference> UploadFileAsync(
            Stream content,
            PostageBatchId batchId,
            ushort? compactLevel = 0,
            string? name = null,
            string? contentType = null,
            bool isFileCollection = false,
            bool? swarmPin = null,
            bool? swarmEncrypt = null,
            string? swarmIndexDocument = null,
            string? swarmErrorDocument = null,
            RedundancyLevel swarmRedundancyLevel = RedundancyLevel.None,
            CancellationToken cancellationToken = default);

        /// <summary>Upload single owner chunk</summary>
        /// <param name="soc">Single owner chunk</param>
        /// <param name="batchId"></param>
        /// <returns>Reference hash</returns>
        /// <exception cref="BeeNetApiException">A server side error occurred.</exception>
        Task<SwarmHash> UploadSocAsync(
            SwarmSoc soc,
            PostageBatchId? batchId,
            PostageStamp? presignedPostageStamp = null,
            bool? swarmPin = null,
            CancellationToken cancellationToken = default);
    }
}
