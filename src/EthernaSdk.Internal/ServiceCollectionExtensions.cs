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

using Etherna.Authentication.ClientCredentials;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Etherna.Sdk.Internal
{
    public static class ServiceCollectionExtensions
    {
        // Methods.
        /// <summary>
        /// Add the registration of the Etherna internal clients, authenticated against the Etherna SSO server
        /// with the "client credentials" flow. Each client added on the returned builder has its own
        /// <see cref="HttpClient"/>, which attaches the access token of that client and manages its expiration.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="ssoBaseUrl">The Etherna SSO server base url</param>
        /// <param name="requireHttps">Require HTTPS on the SSO base url. Loopback addresses are always exempted</param>
        /// <param name="configureHttpClient">An optional delegate to configure the <see cref="HttpClient"/> of each client</param>
        /// <returns>A builder to add the internal clients</returns>
        public static IEthernaInternalClientsBuilder AddEthernaInternalClients(
            this IServiceCollection services,
            Uri ssoBaseUrl,
            bool requireHttps = true,
            Action<HttpClient>? configureHttpClient = null) =>
            new EthernaInternalClientsBuilder(
                services,
                ssoBaseUrl,
                services.AddEthernaClientCredentials(ssoBaseUrl, requireHttps),
                configureHttpClient);
    }
}
