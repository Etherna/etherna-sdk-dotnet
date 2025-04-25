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
using Etherna.BeeNet.Tools;
using Etherna.Sdk.Gateway.GenClients;
using Etherna.Sdk.Users.Gateway.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChainState = Etherna.Sdk.Users.Gateway.Models.ChainState;

namespace Etherna.Sdk.Users.Gateway.Clients
{
    public interface IEthernaUserGatewayClient
    {
        // Properties.
        IBeeClient BeeClient { get; }
        
        // Methods.
        /// <summary>
        /// Admins can set a free pin period for a resource
        /// </summary>
        /// <param name="hash">The swarm resource hash</param>
        /// <param name="freePinEndOfLife">End of free period. Null for disable</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task AdminSetFreeResourcePinningAsync(
            SwarmHash hash,
            DateTimeOffset? freePinEndOfLife = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Announce chunks upload to help pin the root hash on the same node owning the postage batch
        /// </summary>
        /// <param name="rootHash">The swarm resource hash</param>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        Task AnnounceChunksUploadAsync(
            SwarmHash rootHash,
            PostageBatchId batchId,
            CancellationToken cancellationToken = default);

        /// <param name="resourceHashes">The swarm resource hashes list</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<IDictionary<SwarmHash, bool>> AreResourcesDownloadFundedAsync(
            IEnumerable<SwarmHash> resourceHashes,
            CancellationToken cancellationToken = default);

        /// <summary>Buy a new postage batch.</summary>
        /// <param name="amount">New postage batch amount</param>
        /// <param name="depth">New postage batch depth</param>
        /// <param name="label">New postage batch label</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A temporary postage batch reference Id</returns>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<string> BuyPostageBatchAsync(
            BzzBalance amount,
            int depth,
            string? label = null,
            CancellationToken cancellationToken = default);

        Task ChunksBulkUploadAsync(
            SwarmChunk[] chunks,
            PostageBatchId batchId,
            CancellationToken cancellationToken = default);

        /// <param name="hash">The swarm resource hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<bool> DefundResourceDownloadAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <param name="hash">The swarm resource hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<bool> DefundResourcePinningAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <param name="batchId">Postage batch Id</param>
        /// <param name="depth">New postage batch depth</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task DilutePostageBatchAsync(
            PostageBatchId batchId,
            int depth,
            CancellationToken cancellationToken = default);

        /// <param name="hash">The swarm resource hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task FundResourceDownloadAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <param name="hash">The swarm resource hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task FundResourcePinningAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);
        
        /// <summary>Get referenced data</summary>
        /// <param name="hash">Swarm address reference to content</param>
        /// <returns>Retrieved content specified by reference</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task<Stream> GetBytesAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<ChainState> GetChainStateAsync(CancellationToken cancellationToken = default);

        /// <summary>Get Chunk</summary>
        /// <param name="hash">Swarm address of chunk</param>
        /// <returns>Retrieved chunk content</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task<Stream> GetChunkAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<UserCredit> GetCurrentUserCreditAsync(CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<double> GetDownloadBytePriceAsync(CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<IEnumerable<SwarmHash>> GetDownloadFundedResourcesByUserAsync(CancellationToken cancellationToken = default);

        /// <summary>Get file or index document from a collection of files</summary>
        /// <param name="address">Swarm address of content</param>
        /// <param name="swarmCache">Determines if the download data should be cached on the node. By default the download will be cached</param>
        /// <param name="swarmRedundancyStrategy">Specify the retrieve strategy on redundant data. The numbers stand for NONE, DATA, PROX and RACE, respectively. Strategy NONE means no prefetching takes place. Strategy DATA means only data chunks are prefetched. Strategy PROX means only chunks that are close to the node are prefetched. Strategy RACE means all chunks are prefetched: n data chunks and k parity chunks. The first n chunks to arrive are used to reconstruct the file. Multiple strategies can be used in a fallback cascade if the swarm redundancy fallback mode is set to true. The default strategy is NONE, DATA, falling back to PROX, falling back to RACE</param>
        /// <param name="swarmRedundancyFallbackMode">Specify if the retrieve strategies (chunk prefetching on redundant data) are used in a fallback cascade. The default is true.</param>
        /// <param name="swarmChunkRetrievalTimeout">Specify the timeout for chunk retrieval. The default is 30 seconds.</param>
        /// <returns>Ok</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task<FileResponse> GetFileAsync(
            SwarmAddress address,
            CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<IEnumerable<SwarmHash>> GetPinFundedResourcesAsync(CancellationToken cancellationToken = default);

        /// <param name="batchId">Postage batch Id</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<PostageBatch> GetPostageBatchAsync(
            PostageBatchId batchId,
            CancellationToken cancellationToken = default);

        /// <param name="labelContainsFilter">Filter only postage batches with label containing this string. Optional</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<IEnumerable<PostageBatchRef>> GetOwnedPostageBatchesAsync(
            string? labelContainsFilter = null,
            CancellationToken cancellationToken = default);

        /// <param name="hash">The swarm resource hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<ResourcePinStatus> GetResourcePinStatusAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <param name="hash">The swarm resource hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<IEnumerable<string>> GetUsersFundingResourceDownloadAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <param name="hash">The swarm resource hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<IEnumerable<string>> GetUsersFundingResourcePinningAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<WelcomePack> GetWelcomePackInfoAsync(CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task RequireWelcomePackAsync(CancellationToken cancellationToken = default);

        /// <summary>Send to recipient or target with Postal Service for Swarm</summary>
        /// <param name="topic">Topic name</param>
        /// <param name="targets">Target message address prefix. If multiple targets are specified, only one would be matched.</param>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="recipient">Recipient publickey</param>
        /// <returns>Subscribed to topic</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task SendPssAsync(
            string topic,
            string targets,
            PostageBatchId batchId,
            string? recipient = null,
            CancellationToken cancellationToken = default);

        /// <param name="batchId">The postage batch Id</param>
        /// <param name="amount">The amount to top up</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task TopUpPostageBatchAsync(
            PostageBatchId batchId,
            BzzBalance amount,
            CancellationToken cancellationToken = default);

        /// <summary>Find feed update</summary>
        /// <param name="owner">Owner</param>
        /// <param name="topic">Topic</param>
        /// <param name="at">Timestamp of the update (default: now)</param>
        /// <param name="after">Start index (default: 0)</param>
        /// <param name="type">Feed indexing scheme (default: sequence)</param>
        /// <returns>Latest feed update</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task<FileResponse?> TryGetFeedAsync(
            EthAddress owner,
            SwarmFeedTopic topic,
            long? at = null,
            ulong? after = null,
            SwarmFeedType type = SwarmFeedType.Sequence,
            bool? swarmOnlyRootChunk = null,
            CancellationToken cancellationToken = default);

        /// <param name="postageReferenceId">Postage batch reference Id</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>New postage batch Id</returns>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<PostageBatchId?> TryGetNewPostageBatchIdFromPostageRefAsync(
            string postageReferenceId,
            CancellationToken cancellationToken = default);

        /// <summary>Upload data</summary>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="swarmTag">Associate upload with an existing Tag UID</param>
        /// <param name="swarmPin">Represents if the uploaded data should be also locally pinned on the node.
        /// <br/>Warning! Not available for nodes that run in Gateway mode!</param>
        /// <param name="swarmEncrypt">Represents the encrypting state of the file
        /// <br/>Warning! Not available for nodes that run in Gateway mode!</param>
        /// <param name="swarmDeferredUpload">Determines if the uploaded data should be sent to the network immediately or in a deferred fashion. By default the upload will be deferred.</param>
        /// <returns>Reference hash</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task<SwarmHash> UploadBytesAsync(
            PostageBatchId batchId,
            Stream content,
            bool swarmPin = false,
            CancellationToken cancellationToken = default);

        /// <summary>Upload Chunk</summary>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="chunkData"></param>
        /// <param name="swarmPin">Represents if the uploaded data should be also locally pinned on the node.
        ///     <br/>Warning! Not available for nodes that run in Gateway mode!</param>
        /// <param name="swarmDeferredUpload">Determines if the uploaded data should be sent to the network immediately or in a deferred fashion. By default the upload will be deferred.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="swarmTag">Associate upload with an existing Tag UID</param>
        /// <returns>Ok</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task<SwarmHash> UploadChunkAsync(
            Stream chunkData,
            PostageBatchId? batchId,
            bool pinChunk = false,
            TagId? tagId = null,
            PostageStamp? presignedPostageStamp = null,
            CancellationToken cancellationToken = default);

        /// <summary>Upload a directory</summary>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="directoryPath">The directory path</param>
        /// <param name="swarmPin">Represents if the uploaded data should be also locally pinned on the node.</param>
        /// <param name="swarmDeferredUpload">Determines if the uploaded data should be sent to the network immediately or in a deferred fashion. By default the upload will be deferred.</param>
        /// <param name="swarmRedundancyLevel">Add redundancy to the data being uploaded so that downloaders can download it with better UX. 0 value is default and does not add any redundancy to the file.</param>
        /// <returns>Reference hash</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task<SwarmHash> UploadDirectoryAsync(
            PostageBatchId batchId,
            string directoryPath,
            bool pinDirectory = false,
            CancellationToken cancellationToken = default);

        /// <summary>Create an initial feed root manifest</summary>
        /// <param name="feed">Feed</param>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="swarmPin">Represents if the uploaded data should be also locally pinned on the node.
        /// <br/>Warning! Not available for nodes that run in Gateway mode!</param>
        /// <returns>Reference hash</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task<SwarmHash> UploadFeedManifestAsync(
            SwarmFeedBase feed,
            PostageBatchId batchId,
            bool pinManifest = false,
            CancellationToken cancellationToken = default);

        /// <summary>Upload a file</summary>
        /// <param name="batchId">ID of Postage Batch that is used to upload data with</param>
        /// <param name="content">Input file content</param>
        /// <param name="name">Filename when uploading single file</param>
        /// <param name="contentType">The specified content-type is preserved for download of the asset</param>
        /// <param name="swarmPin">Represents if the uploaded data should be also locally pinned on the node.</param>
        /// <param name="swarmDeferredUpload">Determines if the uploaded data should be sent to the network immediately or in a deferred fashion. By default the upload will be deferred.</param>
        /// <param name="swarmRedundancyLevel">Add redundancy to the data being uploaded so that downloaders can download it with better UX. 0 value is default and does not add any redundancy to the file.</param>
        /// <returns>Reference hash</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task<SwarmHash> UploadFileAsync(
            PostageBatchId batchId,
            Stream content,
            string? name = null,
            string? contentType = null,
            bool pinFile = false,
            CancellationToken cancellationToken = default);

        /// <summary>Upload single owner chunk</summary>
        /// <param name="owner">Owner</param>
        /// <param name="id">Id</param>
        /// <param name="signature">Signature</param>
        /// <param name="content">The SOC binary data is composed of the span (8 bytes) and the at most 4KB payload.</param>
        /// <param name="swarmPin">Represents if the uploaded data should be also locally pinned on the node.</param>
        /// <returns>Reference hash</returns>
        /// <exception cref="BeeNetGatewayApiException">A server side error occurred.</exception>
        Task<SwarmHash> UploadSocAsync(
            SwarmSoc soc,
            PostageBatchId? batchId,
            PostageStamp? presignedPostageStamp = null,
            CancellationToken cancellationToken = default);
    }
}