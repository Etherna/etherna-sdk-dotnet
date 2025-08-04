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
using Etherna.Sdk.Users.Gateway.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ChainState = Etherna.Sdk.Users.Gateway.Models.ChainState;
using FileResponse = Etherna.BeeNet.Models.FileResponse;

namespace Etherna.Sdk.Users.Gateway.Clients
{
    public sealed class EthernaUserGatewayClient : IEthernaUserGatewayClient
    {
        // Fields.
        private readonly ChunksClient generatedChunksClient;
        private readonly PostageClient generatedPostageClient;
        private readonly ResourcesClient generatedResourcesClient;
        private readonly SystemClient generatedSystemClient;
        private readonly UsersClient generatedUsersClient;

        // Constructor.
        public EthernaUserGatewayClient(
            Uri baseUrl,
            IBeeClient beeClient,
            HttpClient httpClient)
        {
            ArgumentNullException.ThrowIfNull(baseUrl, nameof(baseUrl));

            BeeClient = beeClient;
            generatedChunksClient = new(baseUrl.AbsoluteUri, httpClient);
            generatedPostageClient = new(baseUrl.AbsoluteUri, httpClient);
            generatedResourcesClient = new(baseUrl.AbsoluteUri, httpClient);
            generatedSystemClient = new(baseUrl.AbsoluteUri, httpClient);
            generatedUsersClient = new(baseUrl.AbsoluteUri, httpClient);
        }
        
        // Properties.
        public IBeeClient BeeClient { get; }

        // Methods.
        public Task AdminSetFreeResourcePinningAsync(
            SwarmHash hash,
            DateTimeOffset? freePinEndOfLife = null,
            CancellationToken cancellationToken = default) =>
            generatedResourcesClient.FreeAsync(hash.ToString(), freePinEndOfLife, cancellationToken);

        public async Task<IDictionary<SwarmHash, bool>> AreResourcesDownloadFundedAsync(
            IEnumerable<SwarmHash> resourceHashes,
            CancellationToken cancellationToken = default) =>
            (await generatedResourcesClient.AreofferedAsync(
                resourceHashes.Select(h => h.ToString()),
                cancellationToken).ConfigureAwait(false))
            .ToDictionary(pair => new SwarmHash(pair.Key), pair => pair.Value);

        public Task<string> BuyPostageBatchAsync(
            BzzValue amount,
            int depth,
            string? label = null,
            CancellationToken cancellationToken = default) =>
            generatedUsersClient.BatchesPostAsync(depth, amount.ToPlurLong(), label, cancellationToken);

        public async Task ChunksBulkUploadAsync(
            SwarmChunk[] chunks,
            PostageBatchId batchId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunks, nameof(chunks));
            
            // Build payload.
            List<byte> payload = [];
            for (var j = 0; j < chunks.Length; j++)
            {
                var chunkBytes = chunks[j].GetFullPayload();
                var chunkSizeByteArray = BitConverter.GetBytes((ushort)chunkBytes.Length);

                //chunk size
                payload.AddRange(chunkSizeByteArray);

                //chunk data
                payload.AddRange(chunkBytes.Span);
                
                //check hash
                payload.AddRange(chunks[j].Hash.ToByteArray());
            }
            
            var byteArrayPayload = payload.ToArray();
            using var memoryStream = new MemoryStream(byteArrayPayload);
            
            await generatedChunksClient.ChunksBulkUploadAsync(
                memoryStream,
                swarm_postage_batch_id: batchId.ToString(),
                cancellationToken).ConfigureAwait(false);
        }

        public Task<bool> DefundResourceDownloadAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            generatedResourcesClient.OffersDeleteAsync(hash.ToString(), cancellationToken);

        public Task<bool> DefundResourcePinningAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            generatedResourcesClient.PinDeleteAsync(hash.ToString(), cancellationToken);

        public Task DilutePostageBatchAsync(
            PostageBatchId batchId,
            int depth,
            CancellationToken cancellationToken = default) =>
            generatedUsersClient.DiluteAsync(batchId.ToString(), depth, cancellationToken);

        public Task FundResourceDownloadAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            generatedResourcesClient.OffersPostAsync(hash.ToString(), cancellationToken);

        public Task FundResourcePinningAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            generatedResourcesClient.PinPostAsync(hash.ToString(), cancellationToken);

        public Task<Stream> GetBytesAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            BeeClient.GetBytesAsync(
                hash: hash,
                cancellationToken: cancellationToken);

        public async Task<ChainState> GetChainStateAsync(
            CancellationToken cancellationToken = default) =>
            new(await generatedSystemClient.ChainstateAsync(cancellationToken).ConfigureAwait(false));

        public Task<Stream> GetChunkAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            BeeClient.GetChunkStreamAsync(
                hash: hash,
                cancellationToken: cancellationToken);

        public async Task<UserCredit> GetCurrentUserCreditAsync(
            CancellationToken cancellationToken = default) =>
            new(await generatedUsersClient.CreditAsync(cancellationToken).ConfigureAwait(false));

        public Task<double> GetDownloadBytePriceAsync(
            CancellationToken cancellationToken = default) =>
            generatedSystemClient.BytepriceAsync(cancellationToken);

        public async Task<IEnumerable<SwarmHash>> GetDownloadFundedResourcesByUserAsync(
            CancellationToken cancellationToken = default) =>
            (await generatedUsersClient.OfferedResourcesAsync(cancellationToken).ConfigureAwait(false))
            .Select(hash => new SwarmHash(hash));

        public Task<FileResponse> GetFileAsync(
            SwarmAddress address,
            CancellationToken cancellationToken = default) =>
            BeeClient.GetFileAsync(
                address: address,
                cancellationToken: cancellationToken);

        public async Task<IEnumerable<SwarmHash>> GetPinFundedResourcesAsync(
            CancellationToken cancellationToken = default) =>
            (await generatedUsersClient.PinnedResourcesAsync(cancellationToken).ConfigureAwait(false))
            .Select(h => new SwarmHash(h));

        public async Task<PostageBatch> GetPostageBatchAsync(
            PostageBatchId batchId,
            CancellationToken cancellationToken = default)
        {
            var batchDto = await generatedUsersClient.BatchesGetAsync(
                batchId.ToString(), cancellationToken).ConfigureAwait(false);
            return new PostageBatch(
                batchDto.Id,
                BzzValue.FromPlurLong(batchDto.Value ?? 0),
                (ulong)(batchDto.BlockNumber ?? 0),
                batchDto.Depth,
                batchDto.Exists ?? false,
                batchDto.ImmutableFlag ?? false,
                batchDto.Usable,
                batchDto.Label,
                TimeSpan.FromSeconds(batchDto.BatchTTL ?? 0),
                (uint)(batchDto.Utilization ?? 0));
        }

        public async Task<IEnumerable<PostageBatchRef>> GetOwnedPostageBatchesAsync(
            string? labelContainsFilter = null,
            CancellationToken cancellationToken = default) =>
            (await generatedUsersClient.BatchesGetSearchAsync(labelContainsFilter, cancellationToken).ConfigureAwait(false))
            .Select(pbr => new PostageBatchRef(pbr));

        public async Task<ResourcePinStatus> GetResourcePinStatusAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            new(await generatedResourcesClient.PinGetAsync(hash.ToString(), cancellationToken).ConfigureAwait(false));

        public async Task<IEnumerable<string>> GetUsersFundingResourceDownloadAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            await generatedResourcesClient.OffersGetAsync(hash.ToString(), cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<string>> GetUsersFundingResourcePinningAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            await generatedResourcesClient.UsersAsync(hash.ToString(), cancellationToken).ConfigureAwait(false);

        public async Task<WelcomePack> GetWelcomePackInfoAsync(
            CancellationToken cancellationToken = default) =>
            new(await generatedUsersClient.WelcomeGetAsync(cancellationToken).ConfigureAwait(false));

        public Task RequireWelcomePackAsync(
            CancellationToken cancellationToken = default) =>
            generatedUsersClient.WelcomePostAsync(cancellationToken);

        public Task SendPssAsync(
            string topic,
            string targets,
            PostageBatchId batchId,
            string? recipient = null,
            CancellationToken cancellationToken = default) =>
            BeeClient.SendPssAsync(topic, targets, batchId, recipient, cancellationToken);

        public Task TopUpPostageBatchAsync(
            PostageBatchId batchId,
            BzzValue amount,
            CancellationToken cancellationToken = default) =>
            generatedPostageClient.TopupAsync(batchId.ToString(), amount.ToPlurLong(), cancellationToken);

        public async Task<FileResponse?> TryGetFeedAsync(
            EthAddress owner,
            SwarmFeedTopic topic,
            long? at = null,
            ulong? after = null,
            SwarmFeedType type = SwarmFeedType.Sequence,
            bool? swarmOnlyRootChunk = null,
            CancellationToken cancellationToken = default) =>
            await BeeClient.TryGetFeedAsync(
                owner: owner,
                topic: topic,
                at: at,
                after: after,
                type: type,
                swarmOnlyRootChunk: swarmOnlyRootChunk,
                cancellationToken: cancellationToken).ConfigureAwait(false);

        public async Task<PostageBatchId?> TryGetNewPostageBatchIdFromPostageRefAsync(
            string postageReferenceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await generatedSystemClient.PostageBatchRefAsync(
                    postageReferenceId,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (EthernaGatewayApiException e) when(e.StatusCode == 404)
            {
                return null;
            }
        }

        public Task<SwarmHash> UploadBytesAsync(
            Stream content,
            PostageBatchId batchId,
            bool swarmPin = false,
            CancellationToken cancellationToken = default) =>
            BeeClient.UploadBytesAsync(
                content,
                batchId,
                swarmPin: swarmPin,
                cancellationToken: cancellationToken);

        public Task<SwarmHash> UploadChunkAsync(
            Stream chunkData,
            PostageBatchId? batchId,
            bool pinChunk = false,
            TagId? tagId = null,
            PostageStamp? presignedPostageStamp = null,
            CancellationToken cancellationToken = default) =>
            BeeClient.UploadChunkAsync(
                chunkData: chunkData,
                batchId: batchId,
                pinChunk: pinChunk,
                tagId: tagId,
                presignedPostageStamp: presignedPostageStamp,
                cancellationToken: cancellationToken);
        
        public Task<SwarmHash> UploadDirectoryAsync(
            string directoryPath,
            PostageBatchId batchId,
            bool pinDirectory = false,
            CancellationToken cancellationToken = default) =>
            BeeClient.UploadDirectoryAsync(
                directoryPath,
                batchId,
                swarmPin: pinDirectory,
                cancellationToken: cancellationToken);

        public Task<SwarmHash> UploadFeedManifestAsync(
            SwarmFeedBase feed,
            PostageBatchId batchId,
            bool pinManifest = false,
            CancellationToken cancellationToken = default) =>
            BeeClient.UploadFeedManifestAsync(
                feed: feed,
                batchId: batchId,
                swarmPin: pinManifest,
                cancellationToken: cancellationToken);

        public Task<SwarmHash> UploadFileAsync(
            Stream content,
            PostageBatchId batchId,
            string? name = null,
            string? contentType = null,
            bool pinFile = false,
            CancellationToken cancellationToken = default) =>
            BeeClient.UploadFileAsync(
                content,
                batchId,
                name: name,
                contentType: contentType,
                swarmPin: pinFile,
                cancellationToken: cancellationToken);

        public Task<SwarmHash> UploadSocAsync(
            SwarmSoc soc,
            PostageBatchId? batchId,
            PostageStamp? presignedPostageStamp = null,
            CancellationToken cancellationToken = default) =>
            BeeClient.UploadSocAsync(
                soc: soc,
                batchId: batchId,
                presignedPostageStamp: presignedPostageStamp,
                cancellationToken: cancellationToken);
    }
}
