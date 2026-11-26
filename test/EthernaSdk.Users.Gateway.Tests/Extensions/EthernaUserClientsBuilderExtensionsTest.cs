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

using Etherna.Authentication;
using Etherna.Authentication.Native;
using Etherna.SwarmSdk;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Users.Gateway.Extensions
{
    public class EthernaUserClientsBuilderExtensionsTest
    {
        // Consts.
        /// <summary>
        /// An unsigned access token of the user "user": the sign in reads its claims without validating it
        /// </summary>
        private const string AccessToken = "eyJhbGciOiJub25lIn0.eyJzdWIiOiJ1c2VyIn0.";

        // Tests.
        [Fact]
        public void AddEthernaGatewayClient_BuildsSwarmClientOverSharedHttpClientWithDefaultTimeout()
        {
            // Setup.
            var services = new ServiceCollection();
            services.AddEthernaUserClients(
                    clientId: "clientId",
                    clientSecret: null,
                    returnUrlPort: 3000)
                .AddEthernaGatewayClient();
            using var serviceProvider = services.BuildServiceProvider();

            // Action.
            var swarmClient = serviceProvider.GetRequiredService<ISwarmClient>();

            // Assert.
            Assert.Equal(new Uri(EthernaUserClientsBuilder.DefaultGatewayUrl), swarmClient.NodeUrl);

            //the shared named client keeps its default timeout for the rest clients
            var sharedHttpClient = serviceProvider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(ServiceCollectionExtensions.DefaultEthernaUserHttpClientName);
            Assert.Equal(TimeSpan.FromSeconds(100), sharedHttpClient.Timeout);
        }

        [Fact]
        public async Task AddEthernaGatewayClient_LeavesCustomScopesUnchanged()
        {
            // Setup.
            var requestedScopes = new List<string>();
            await using var ssoServer = await StartSsoServerAsync(requestedScopes);
            var services = new ServiceCollection();
            services.AddEthernaUserClients(
                    clientId: "clientId",
                    clientSecret: null,
                    returnUrlPort: 3000,
                    scopes: ["customScope"],
                    authority: ssoServer.Urls.Single() + "/")
                .AddEthernaGatewayClient();
            services.ConfigureAll<OpenIdConnectOptions>(options => options.RequireHttpsMetadata = false); //the SSO server is plain http
            await using var serviceProvider = services.BuildServiceProvider();

            // Action.
            await serviceProvider.GetRequiredService<IEthernaSignInService>().SignInAsync("user.secret");

            // Assert.
            //the api key flow requests the custom scopes at sign in
            var apiKeyFlowScopes = Assert.Single(requestedScopes).Split(' ');
            Assert.Contains("customScope", apiKeyFlowScopes);
            Assert.DoesNotContain(EthernaScopes.UserApiGateway, apiKeyFlowScopes);

            //the code flow needs a browser: it requests the scopes in the options of its scheme
            var codeFlowScopes = serviceProvider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>()
                .Get(EthernaNativeDefaults.CodeAuthenticationScheme).Scope;
            Assert.Contains("customScope", codeFlowScopes);
            Assert.DoesNotContain(EthernaScopes.UserApiGateway, codeFlowScopes);
        }

        [Fact]
        public async Task AddEthernaGatewayClient_RequestsTheGatewayScopeAtSignIn()
        {
            // Setup.
            var requestedScopes = new List<string>();
            await using var ssoServer = await StartSsoServerAsync(requestedScopes);
            var services = new ServiceCollection();
            services.AddEthernaUserClients(
                    clientId: "clientId",
                    clientSecret: null,
                    returnUrlPort: 3000,
                    authority: ssoServer.Urls.Single() + "/")
                .AddEthernaGatewayClient();
            services.ConfigureAll<OpenIdConnectOptions>(options => options.RequireHttpsMetadata = false); //the SSO server is plain http
            await using var serviceProvider = services.BuildServiceProvider();

            // Action.
            await serviceProvider.GetRequiredService<IEthernaSignInService>().SignInAsync("user.secret");

            // Assert.
            //the api key flow requests the scopes at sign in
            Assert.Contains(EthernaScopes.UserApiGateway, Assert.Single(requestedScopes).Split(' '));

            //the code flow needs a browser: it requests the scopes in the options of its scheme
            Assert.Contains(
                EthernaScopes.UserApiGateway,
                serviceProvider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>()
                    .Get(EthernaNativeDefaults.CodeAuthenticationScheme).Scope);
        }

        // Helpers.
        /// <summary>
        /// Start an SSO server on loopback, answering the discovery and the token requests, and recording the
        /// scope of each token request
        /// </summary>
        private static async Task<WebApplication> StartSsoServerAsync(List<string> requestedScopes)
        {
            var builder = WebApplication.CreateSlimBuilder();
            builder.Logging.ClearProviders();
            builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
            var ssoServer = builder.Build();

            ssoServer.MapGet("/.well-known/openid-configuration", (HttpRequest request) =>
            {
                var authority = $"{request.Scheme}://{request.Host}";
                return Results.Text(
                    $$"""
                    {
                      "issuer": "{{authority}}",
                      "authorization_endpoint": "{{authority}}/connect/authorize",
                      "token_endpoint": "{{authority}}/connect/token",
                      "jwks_uri": "{{authority}}/.well-known/jwks"
                    }
                    """,
                    "application/json");
            });
            ssoServer.MapGet("/.well-known/jwks", () => Results.Text("""{ "keys": [] }""", "application/json"));
            ssoServer.MapPost("/connect/token", async (HttpRequest request) =>
            {
                var form = await request.ReadFormAsync();
                var scope = form["scope"].ToString();
                requestedScopes.Add(scope);
                return Results.Text(
                    $$"""
                    {
                      "access_token": "{{AccessToken}}",
                      "token_type": "Bearer",
                      "expires_in": 3600,
                      "refresh_token": "refreshToken",
                      "scope": "{{scope}}"
                    }
                    """,
                    "application/json");
            });

            await ssoServer.StartAsync();
            return ssoServer;
        }
    }
}
