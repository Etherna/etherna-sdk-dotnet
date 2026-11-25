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
using Etherna.Sdk.Internal.Clients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Etherna.Sdk.Internal
{
    public sealed class EthernaInternalClientsBuilder(
        IServiceCollection services,
        Uri ssoBaseUrl,
        IEthernaClientCredentialsBuilder clientCredentialsBuilder,
        Action<HttpClient>? configureHttpClient)
        : IEthernaInternalClientsBuilder
    {
        // Consts.
        public const string EthernaInternalCreditHttpClientName = "ethernaInternalCreditHttpClient";
        public const string EthernaInternalSsoHttpClientName = "ethernaInternalSsoHttpClient";
        
        // Private consts.
        private const string CreditServiceInteractScope = "ethernaCredit_serviceInteract_api";
        private const string SsoUserContactInfoScope = "ethernaSso_userContactInfo_api";
        private const string EthernaInternalCreditTokenClientName = "ethernaInternalCreditTokenClient";
        private const string EthernaInternalSsoTokenClientName = "ethernaInternalSsoTokenClient";

        // Methods.
        public IEthernaInternalClientsBuilder AddEthernaCreditClient(
            Uri creditServiceBaseUrl,
            string clientId,
            string clientSecret)
        {
            ArgumentNullException.ThrowIfNull(creditServiceBaseUrl);

            return AddClient<IEthernaInternalCreditClient>(
                EthernaInternalCreditTokenClientName,
                clientId,
                clientSecret,
                CreditServiceInteractScope,
                EthernaInternalCreditHttpClientName,
                httpClient => new EthernaInternalCreditClient(creditServiceBaseUrl, httpClient));
        }

        public IEthernaInternalClientsBuilder AddEthernaSsoClient(
            string clientId,
            string clientSecret) =>
            AddClient<IEthernaInternalSsoClient>(
                EthernaInternalSsoTokenClientName,
                clientId,
                clientSecret,
                SsoUserContactInfoScope,
                EthernaInternalSsoHttpClientName,
                httpClient => new EthernaInternalSsoClient(ssoBaseUrl, httpClient));

        // Helpers.
        private EthernaInternalClientsBuilder AddClient<TClient>(
            string tokenClientName,
            string clientId,
            string clientSecret,
            string scope,
            string httpClientName,
            Func<HttpClient, TClient> buildClient)
            where TClient : class
        {
            // Register the token client, and the http client that attaches its access token.
            // Each internal client has its own http client: a shared one would send the token of a single client.
            clientCredentialsBuilder.AddClient(
                tokenClientName,
                clientId,
                clientSecret,
                [scope],
                httpClientName,
                configureHttpClient);

            // Register service.
            services.AddSingleton(serviceProvider =>
                buildClient(serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName)));

            return this;
        }
    }
}
