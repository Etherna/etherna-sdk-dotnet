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

using Etherna.Sdk.Internal.Clients;
using Etherna.SwarmSdk.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Internal
{
    public class ServiceCollectionExtensionsTest
    {
        // Consts.
        private const string CreditClientId = "creditClientId";
        private const string CreditServiceUrl = "https://credit.test/";
        private const string SsoClientId = "ssoClientId";
        private const string SsoUrl = "https://sso.test/";
        private static readonly EthAddress UserAddress = EthAddress.FromString("0x0000000000000000000000000000000000000001");

        // Classes.
        /// <summary>
        /// The handler under every http client of a test: it answers the token endpoint of the SSO server with
        /// a token naming the client that asked for it, and records what it receives.
        /// </summary>
        private sealed class RecordingHttpMessageHandler(
            List<(string Host, string? Bearer)> apiRequests,
            List<(Uri Endpoint, string ClientId, string Scope)> tokenRequests)
            : HttpMessageHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                if (request.RequestUri!.AbsolutePath != "/connect/token")
                {
                    apiRequests.Add((request.RequestUri.Host, request.Headers.Authorization?.Parameter));
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{}", Encoding.UTF8, "application/json")
                    };
                }

                //the client authenticates with the basic scheme, or in the form
                var form = (await request.Content!.ReadAsStringAsync(cancellationToken))
                    .Split('&')
                    .Select(pair => pair.Split('='))
                    .ToDictionary(pair => pair[0], pair => Uri.UnescapeDataString(pair[1]));
                var clientId = request.Headers.Authorization is { Scheme: "Basic", Parameter: { } credentials } ?
                    Uri.UnescapeDataString(Encoding.UTF8.GetString(Convert.FromBase64String(credentials)).Split(':')[0]) :
                    form["client_id"];
                tokenRequests.Add((request.RequestUri, clientId, form["scope"]));

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        $$"""{"access_token":"token-of-{{clientId}}","token_type":"Bearer","expires_in":3600}""",
                        Encoding.UTF8,
                        "application/json")
                };
            }
        }

        // Fields.
        private readonly List<(string Host, string? Bearer)> apiRequests = [];
        private readonly List<(Uri Endpoint, string ClientId, string Scope)> tokenRequests = [];

        // Tests.
        [Fact]
        public void AddEthernaInternalClients_ConfiguresHttpClientOfEachClient()
        {
            // Setup.
            var services = BuildServices();
            services.AddEthernaInternalClients(
                    new Uri(SsoUrl),
                    configureHttpClient: httpClient => httpClient.Timeout = TimeSpan.FromSeconds(42))
                .AddEthernaCreditClient(new Uri(CreditServiceUrl), CreditClientId, "creditSecret")
                .AddEthernaSsoClient(SsoClientId, "ssoSecret");
            using var serviceProvider = services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            // Action.
            using var creditHttpClient = httpClientFactory.CreateClient(
                EthernaInternalClientsBuilder.EthernaInternalCreditHttpClientName);
            using var ssoHttpClient = httpClientFactory.CreateClient(
                EthernaInternalClientsBuilder.EthernaInternalSsoHttpClientName);

            // Assert.
            Assert.Equal(TimeSpan.FromSeconds(42), creditHttpClient.Timeout);
            Assert.Equal(TimeSpan.FromSeconds(42), ssoHttpClient.Timeout);
        }

        [Theory]
        [InlineData("http://sso.test/", true, false)]
        [InlineData("http://sso.test/", false, true)]
        [InlineData("http://localhost:5000/", true, true)]
        [InlineData("https://sso.test/", true, true)]
        public void AddEthernaInternalClients_RequiresHttpsOnSsoUrl(string ssoUrl, bool requireHttps, bool isAccepted)
        {
            // Setup.
            var services = BuildServices();

            // Action.
            var exception = Record.Exception(() => services.AddEthernaInternalClients(new Uri(ssoUrl), requireHttps));

            // Assert.
            if (isAccepted)
                Assert.Null(exception);
            else
                Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task AddEthernaInternalClients_RequestsTheTokenOfAClientOnce()
        {
            // Setup.
            var services = BuildServices();
            services.AddEthernaInternalClients(new Uri(SsoUrl))
                .AddEthernaCreditClient(new Uri(CreditServiceUrl), CreditClientId, "creditSecret");
            await using var serviceProvider = services.BuildServiceProvider();
            var creditClient = serviceProvider.GetRequiredService<IEthernaInternalCreditClient>();

            // Action.
            await creditClient.GetUserCreditAsync(UserAddress);
            await creditClient.GetUserCreditAsync(UserAddress);

            // Assert.
            Assert.Single(tokenRequests);
            Assert.Equal(2, apiRequests.Count);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public async Task AddEthernaInternalClients_SendsWithEachClientItsOwnToken(
            bool addCreditClient,
            bool addSsoClient,
            bool addSsoClientFirst)
        {
            // Setup.
            var services = BuildServices();
            var builder = services.AddEthernaInternalClients(new Uri(SsoUrl));
            if (addSsoClient && addSsoClientFirst)
                builder.AddEthernaSsoClient(SsoClientId, "ssoSecret");
            if (addCreditClient)
                builder.AddEthernaCreditClient(new Uri(CreditServiceUrl), CreditClientId, "creditSecret");
            if (addSsoClient && !addSsoClientFirst)
                builder.AddEthernaSsoClient(SsoClientId, "ssoSecret");
            await using var serviceProvider = services.BuildServiceProvider();

            // Action.
            if (addCreditClient)
                await serviceProvider.GetRequiredService<IEthernaInternalCreditClient>().GetUserCreditAsync(UserAddress);
            if (addSsoClient)
                await serviceProvider.GetRequiredService<IEthernaInternalSsoClient>().GetUserContactsAsync(UserAddress);

            // Assert.
            var expectedApiRequests = new List<(string, string?)>();
            var expectedTokenRequests = new List<(Uri, string, string)>();
            if (addCreditClient)
            {
                expectedApiRequests.Add(("credit.test", $"token-of-{CreditClientId}"));
                expectedTokenRequests.Add((new Uri("https://sso.test/connect/token"), CreditClientId, "ethernaCredit_serviceInteract_api"));
            }
            if (addSsoClient)
            {
                expectedApiRequests.Add(("sso.test", $"token-of-{SsoClientId}"));
                expectedTokenRequests.Add((new Uri("https://sso.test/connect/token"), SsoClientId, "ethernaSso_userContactInfo_api"));
            }
            Assert.Equal(expectedApiRequests, apiRequests);
            Assert.Equal(expectedTokenRequests, tokenRequests);
        }

        // Helpers.
        private ServiceCollection BuildServices()
        {
            var services = new ServiceCollection();
            services.ConfigureAll<HttpClientFactoryOptions>(options =>
                options.HttpMessageHandlerBuilderActions.Add(handlerBuilder =>
                    handlerBuilder.PrimaryHandler = new RecordingHttpMessageHandler(apiRequests, tokenRequests)));
            return services;
        }
    }
}
