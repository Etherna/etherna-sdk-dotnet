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

using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Etherna.Sdk.Internal.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        // Consts.
        public const string DefaultEthernaInternalHttpClientName = "ethernaInternalHttpClient";

        // Methods.
        public static IEthernaInternalClientsBuilder AddEthernaInternalClients(
            this IServiceCollection services,
            Uri ssoBaseUrl,
            bool requireHttps = true,
            string httpClientName = DefaultEthernaInternalHttpClientName,
            Action<HttpClient>? configureHttpClient = null)
        {
            ArgumentNullException.ThrowIfNull(ssoBaseUrl);

            // Register memory cache to keep tokens.
            services.AddHybridCache();

            // Register client token management.
            var clientCredentialsTokenManagementBuilder = services.AddClientCredentialsTokenManagement();

            // Discover token endpoint.
            var discoverTokenEndpointTask = DiscoverTokenEndpointAsync(requireHttps, ssoBaseUrl);
            discoverTokenEndpointTask.Wait();
            var tokenEndpoint = discoverTokenEndpointTask.Result;

            return new EthernaInternalClientsBuilder(
                services,
                ssoBaseUrl,
                tokenEndpoint,
                clientCredentialsTokenManagementBuilder,
                httpClientName,
                configureHttpClient);
        }

        // Helpers.
        private static async Task<string> DiscoverTokenEndpointAsync(
            bool requireHttps,
            Uri ssoBaseUrl)
        {
            // Discover endpoints from metadata.
            using var httpClient = new HttpClient();
            using var request = new DiscoveryDocumentRequest
            {
                Address = ssoBaseUrl.AbsoluteUri,
                Policy = new DiscoveryPolicy { RequireHttps = requireHttps }
            };

            var discoveryDocResult = await httpClient.GetDiscoveryDocumentAsync(request).ConfigureAwait(false);

            if (discoveryDocResult.IsError)
                throw discoveryDocResult.Exception ?? new InvalidOperationException();

            return discoveryDocResult.TokenEndpoint!;
        }
    }
}
