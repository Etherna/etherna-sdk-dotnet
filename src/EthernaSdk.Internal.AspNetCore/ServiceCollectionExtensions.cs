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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

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
            if (requireHttps &&
                ssoBaseUrl.Scheme != Uri.UriSchemeHttps &&
                !ssoBaseUrl.IsLoopback)
                throw new ArgumentException("HTTPS is required for the SSO base URL.", nameof(ssoBaseUrl));

            // Register memory cache to keep tokens.
            services.AddHybridCache();

            // Register client token management.
            var clientCredentialsTokenManagementBuilder = services.AddClientCredentialsTokenManagement();

            // Build token endpoint from IdentityServer's conventional route, without network discovery.
            // This way the SSO server is not required to be reachable when the application starts.
            var tokenEndpoint = ssoBaseUrl.AbsoluteUri.TrimEnd('/') + "/connect/token";

            return new EthernaInternalClientsBuilder(
                services,
                ssoBaseUrl,
                tokenEndpoint,
                clientCredentialsTokenManagementBuilder,
                httpClientName,
                configureHttpClient);
        }
    }
}
