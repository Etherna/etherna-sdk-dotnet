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


using Etherna.SwarmSdk;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Xunit;

namespace Etherna.Sdk.Users.Gateway.Extensions
{
    public class EthernaUserClientsBuilderExtensionsTest
    {
        [Fact]
        public void AddEthernaGatewayClient_BuildsSwarmClientOverSharedHttpClientWithDefaultTimeout()
        {
            // Setup.
            var services = new ServiceCollection();
            services.AddEthernaUserClients(
                    clientId: "clientId",
                    clientSecret: null,
                    returnUrlPort: 3000,
                    scopes: [])
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
    }
}
