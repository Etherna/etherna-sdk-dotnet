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
using Etherna.Sdk.Internal.Clients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Etherna.Sdk.Internal.AspNetCore
{
    internal sealed class EthernaInternalClientsBuilder(
        IServiceCollection services,
        Uri ssoBaseUrl,
        string tokenEndpoint,
        ClientCredentialsTokenManagementBuilder clientCredentialsTokenManagementBuilder,
        string httpClientName,
        Action<HttpClient>? configureHttpClient)
        : IEthernaInternalClientsBuilder
    {
        // Consts.
        private const string EthernaInternalCreditTokenClientName = "ethernaInternalCreditTokenClient";
        private const string EthernaInternalSsoTokenClientName = "ethernaInternalSsoTokenClient";

        // Methods.
        public IEthernaInternalClientsBuilder AddEthernaCreditClient(
            Uri creditServiceBaseUrl,
            string clientId,
            string clientSecret)
        {
            // Register client to token management.
            clientCredentialsTokenManagementBuilder.AddClient(EthernaInternalCreditTokenClientName, options =>
            {
                options.TokenEndpoint = new Uri(tokenEndpoint);

                options.ClientId = ClientId.Parse(clientId);
                options.ClientSecret = ClientSecret.Parse(clientSecret);

                options.Scope = Scope.Parse("ethernaCredit_serviceInteract_api");
            });

            // Register http client.
            services.AddClientCredentialsHttpClient(
                httpClientName,
                ClientCredentialsClientName.Parse(EthernaInternalCreditTokenClientName),
                configureHttpClient);

            // Register service.
            services.AddSingleton<IEthernaInternalCreditClient>(serviceProvider =>
            {
                var clientFactory = serviceProvider.GetService<IHttpClientFactory>()!;
                return new EthernaInternalCreditClient(
                    creditServiceBaseUrl,
                    clientFactory.CreateClient(httpClientName));
            });

            return this;
        }

        public IEthernaInternalClientsBuilder AddEthernaSsoClient(
            string clientId,
            string clientSecret)
        {
            // Register client to token management.
            clientCredentialsTokenManagementBuilder.AddClient(EthernaInternalSsoTokenClientName, options =>
            {
                options.TokenEndpoint = new Uri(tokenEndpoint);

                options.ClientId = ClientId.Parse(clientId);
                options.ClientSecret = ClientSecret.Parse(clientSecret);

                options.Scope = Scope.Parse("ethernaSso_userContactInfo_api");
            });

            // Register http client.
            services.AddClientCredentialsHttpClient(
                httpClientName,
                ClientCredentialsClientName.Parse(EthernaInternalSsoTokenClientName),
                configureHttpClient);

            // Register service.
            services.AddSingleton<IEthernaInternalSsoClient>(serviceProvider =>
            {
                var clientFactory = serviceProvider.GetService<IHttpClientFactory>()!;
                return new EthernaInternalSsoClient(
                    ssoBaseUrl,
                    clientFactory.CreateClient(httpClientName));
            });

            return this;
        }
    }
}
