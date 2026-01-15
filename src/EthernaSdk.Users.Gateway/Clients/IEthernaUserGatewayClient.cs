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
        // Methods.
        /// <summary>
        /// Admins can set a free pin period for a resource
        /// </summary>
        /// <param name="reference">The swarm resource hash</param>
        /// <param name="freePinEndOfLife">End of free period. Null for disable</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task AdminSetFreeResourcePinningAsync(
            SwarmReference reference,
            DateTimeOffset? freePinEndOfLife = null,
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
            BzzValue amount,
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
        
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<ChainState> GetChainStateAsync(CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<UserCredit> GetCurrentUserCreditAsync(CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<double> GetDownloadBytePriceAsync(CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<IEnumerable<SwarmHash>> GetDownloadFundedResourcesByUserAsync(CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<IEnumerable<SwarmReference>> GetPinFundedResourcesAsync(CancellationToken cancellationToken = default);

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

        /// <param name="reference">The swarm resource hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<ResourcePinStatus> GetResourcePinStatusAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default);

        /// <param name="hash">The swarm resource hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<IEnumerable<string>> GetUsersFundingResourceDownloadAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default);

        /// <param name="reference">The swarm resource hash</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<IEnumerable<string>> GetUsersFundingResourcePinningAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<WelcomePack> GetWelcomePackInfoAsync(CancellationToken cancellationToken = default);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task RequireWelcomePackAsync(CancellationToken cancellationToken = default);

        /// <param name="batchId">The postage batch Id</param>
        /// <param name="amount">The amount to top up</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task TopUpPostageBatchAsync(
            PostageBatchId batchId,
            BzzValue amount,
            CancellationToken cancellationToken = default);

        /// <param name="postageReferenceId">Postage batch reference Id</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>New postage batch Id</returns>
        /// <exception cref="EthernaGatewayApiException">A server side error occurred.</exception>
        Task<PostageBatchId?> TryGetNewPostageBatchIdFromPostageRefAsync(
            string postageReferenceId,
            CancellationToken cancellationToken = default);
    }
}