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
using Etherna.Sdk.Users.Gateway.Models;
using Etherna.SwarmSdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Sdk.Users.Gateway.Clients
{
    public sealed class EthernaUserGatewayClient : IEthernaUserGatewayClient
    {
        // Fields.
        private readonly ResourcesClient generatedResourcesClient;
        private readonly SystemClient generatedSystemClient;
        private readonly UsersClient generatedUsersClient;

        // Constructor.
        public EthernaUserGatewayClient(
            Uri baseUrl,
            HttpClient httpClient)
        {
            ArgumentNullException.ThrowIfNull(baseUrl);

            generatedResourcesClient = new(baseUrl.AbsoluteUri, httpClient);
            generatedSystemClient = new(baseUrl.AbsoluteUri, httpClient);
            generatedUsersClient = new(baseUrl.AbsoluteUri, httpClient);
        }

        // Methods.
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

        public Task<bool> DefundResourceDownloadAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default) =>
            generatedResourcesClient.OffersDeleteAsync(reference.ToString(), cancellationToken);

        public Task FundResourceDownloadAsync(
            SwarmReference reference,
            CancellationToken cancellationToken = default) =>
            generatedResourcesClient.OffersPostAsync(reference.ToString(), cancellationToken);

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

        public async Task<IEnumerable<PostageBatchRef>> GetOwnedPostageBatchesAsync(
            string? labelContainsFilter = null,
            CancellationToken cancellationToken = default) =>
            (await generatedUsersClient.BatchesGetSearchAsync(labelContainsFilter, cancellationToken).ConfigureAwait(false))
            .Select(pbr => new PostageBatchRef(pbr));

        public async Task<IEnumerable<string>> GetUsersFundingResourceDownloadAsync(
            SwarmHash hash,
            CancellationToken cancellationToken = default) =>
            await generatedResourcesClient.OffersGetAsync(hash.ToString(), cancellationToken).ConfigureAwait(false);

        public async Task<WelcomePack> GetWelcomePackInfoAsync(
            CancellationToken cancellationToken = default) =>
            new(await generatedUsersClient.WelcomeGetAsync(cancellationToken).ConfigureAwait(false));

        public Task RequireWelcomePackAsync(
            CancellationToken cancellationToken = default) =>
            generatedUsersClient.WelcomePostAsync(cancellationToken);

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
    }
}
