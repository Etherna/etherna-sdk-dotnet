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

using Etherna.Authentication.Native;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Etherna.Sdk.Users
{
    public static class ServiceCollectionExtensions
    {
        // Consts.
        public const string DefaultEthernaUserHttpClientName = "ethernaUserHttpClient";

        // Methods.
        /// <summary>
        /// Register the sign in to the Etherna SSO server, with both the native flows, and return the builder
        /// that adds the user clients
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="clientId">The client id on the SSO server, used by the interactive sign in</param>
        /// <param name="clientSecret">The optional client secret</param>
        /// <param name="returnUrlPort">The loopback port of the client redirect url on the SSO server</param>
        /// <param name="scopes">The api scopes to request at sign in, as they are. When null, the default ones:
        /// the scopes of the services of the registered clients</param>
        /// <param name="authority">The SSO server url</param>
        /// <param name="httpClientName">The name of the http client shared by the user clients</param>
        /// <param name="configureHttpClient">An optional configuration of the shared http client</param>
        /// <returns>The builder of the user clients</returns>
        public static IEthernaUserClientsBuilder AddEthernaUserClients(
            this IServiceCollection services,
            string clientId,
            string? clientSecret,
            int returnUrlPort,
            IEnumerable<string>? scopes = null,
            string authority = EthernaUserClientsBuilder.DefaultSsoUrl,
            string httpClientName = DefaultEthernaUserHttpClientName,
            Action<HttpClient>? configureHttpClient = null)
        {
            // Register Etherna OpenId Connect client with all the sign in flows. The flow is chosen
            // at sign in time with the IEthernaSignInService overloads, eventually passing an api key.
            services.AddEthernaOidcClient(
                authority,
                clientId,
                clientSecret,
                returnUrlPort,
                scopes ?? [],
                httpClientName,
                configureHttpClient);

            // Without custom scopes, each client registration adds the default scope of its service.
            return new EthernaUserClientsBuilder(
                services,
                httpClientName,
                new Uri(authority))
            {
                HasCustomScopes = scopes is not null
            };
        }

        // Internal methods.
        /// <summary>
        /// Try to request the default api scope of a registered client at sign in, the one of the service it
        /// calls. It is not added when the sign in has custom scopes, which are requested as they are.
        /// </summary>
        /// <param name="builder">The builder of the user clients</param>
        /// <param name="scope">The default api scope of the client</param>
        /// <returns>True if the scope is added, false if the sign in has custom scopes</returns>
        internal static bool TryAddEthernaUserDefaultApiScope(this IEthernaUserClientsBuilder builder, string scope)
        {
            // Only the builder of AddEthernaUserClients can carry custom scopes.
            if (builder is EthernaUserClientsBuilder { HasCustomScopes: true })
                return false;

            // Both sign in flows read the scopes to request from the options of their scheme, when the sign in
            // services are built: after every registration, so also after this one.
            builder.Services.Configure<OpenIdConnectOptions>(
                EthernaNativeDefaults.ApiKeyAuthenticationScheme,
                options => options.Scope.Add(scope));
            builder.Services.Configure<OpenIdConnectOptions>(
                EthernaNativeDefaults.CodeAuthenticationScheme,
                options => options.Scope.Add(scope));
            return true;
        }
    }
}