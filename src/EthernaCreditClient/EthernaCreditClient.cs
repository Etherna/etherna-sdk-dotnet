﻿using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Etherna.CreditClient
{
    public class EthernaCreditClient : IEthernaCreditClient
    {
        // Fields.
        private readonly Uri baseUrl;
        private readonly Uri ssoBaseUrl;
        private readonly string ssoClientId;
        private readonly string ssoClientSecret;

        // Constructors.
        public EthernaCreditClient(
            Uri baseUrl,
            Uri ssoBaseUrl,
            string ssoClientId,
            string ssoClientSecret)
        {
            if (string.IsNullOrEmpty(ssoClientId))
            {
                throw new ArgumentException($"'{nameof(ssoClientId)}' cannot be null or empty", nameof(ssoClientId));
            }

            if (string.IsNullOrEmpty(ssoClientSecret))
            {
                throw new ArgumentException($"'{nameof(ssoClientSecret)}' cannot be null or empty", nameof(ssoClientSecret));
            }

            this.baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            this.ssoBaseUrl = ssoBaseUrl ?? throw new ArgumentNullException(nameof(ssoBaseUrl));
            this.ssoClientId = ssoClientId;
            this.ssoClientSecret = ssoClientSecret;

            InitializeAsync().Wait();
        }

        // Properties.
        public IServiceInteractClient ServiceInteract { get; private set; } = default!;

        // Helpers.
        private async Task InitializeAsync()
        {
            // Discover endpoints from metadata.
            using var httpClient = new HttpClient();
            var discoveryDoc = await httpClient.GetDiscoveryDocumentAsync(ssoBaseUrl.AbsoluteUri).ConfigureAwait(false);
            if (discoveryDoc.IsError)
                throw discoveryDoc.Exception ?? new InvalidOperationException();

            // Request token.
            using var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = discoveryDoc.TokenEndpoint,

                ClientId = ssoClientId,
                ClientSecret = ssoClientSecret,
                Scope = "ethernaCredit_serviceInteract_api"
            };
            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(tokenRequest).ConfigureAwait(false);

            if (tokenResponse.IsError)
                throw tokenResponse.Exception ?? new InvalidOperationException();

            // Set api token.
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            // Create service clients.
            ServiceInteract = new ServiceInteractClient(baseUrl.AbsoluteUri, apiClient);
        }
    }
}
