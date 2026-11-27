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

using Etherna.Sdk.Users.Index.Clients;
using Etherna.SwarmSdk;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Sdk.Users.Index.Extensions
{
    public class EthernaUserClientsBuilderExtensionsTest
    {
        // Classes.
        /// <summary>
        /// The primary handler of the http client shared by the user clients, the one with the access token of the
        /// user: a request reaches it only through that http client. It answers the parameters request of every
        /// index with a comment max length of its own, and records the requested urls
        /// </summary>
        private sealed class IndexHandler : HttpMessageHandler
        {
            // Fields.
            private static readonly Dictionary<string, int> CommentMaxLengths = new()
            {
                ["index-a.example"] = 100,
                ["index-b.example"] = 200,
                ["index-c.example"] = 300
            };

            // Properties.
            public List<Uri> RequestUris { get; } = [];

            // Static methods.
            public static int GetCommentMaxLength(string host) => CommentMaxLengths[host];

            // Protected methods.
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var uri = request.RequestUri!;
                RequestUris.Add(uri);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        $$"""
                        {
                          "commentMaxLength": {{GetCommentMaxLength(uri.Host)}},
                          "videoDescriptionMaxLength": 5000,
                          "videoPersonalDataMaxLength": 200,
                          "videoTitleMaxLength": 150
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                });
            }
        }

        // Tests.
        [Fact]
        public void AddEthernaIndexClient_RegistersNoClientOfADefaultIndex()
        {
            // Setup.
            using var serviceProvider = BuildServiceProvider(new IndexHandler());

            // Action.
            var indexClient = serviceProvider.GetService<IEthernaUserIndexClient>();

            // Assert.
            Assert.Null(indexClient);
        }

        [Fact]
        public async Task AddEthernaIndexClient_RegistersAFactoryOfClientsOfDifferentIndexes()
        {
            // Setup.
            var indexHandler = new IndexHandler();
            using var serviceProvider = BuildServiceProvider(indexHandler);
            var indexClientFactory = serviceProvider.GetRequiredService<IEthernaUserIndexClientFactory>();

            // Action.
            var indexAClient = indexClientFactory.Create(new Uri("https://index-a.example/"));
            var indexBClient = indexClientFactory.Create(new Uri("https://index-b.example/"));
            var indexCClient = indexClientFactory.Create(new Uri("https://index-c.example/"));
            var indexBParameters = await indexBClient.GetIndexParametersAsync();
            var indexAParameters = await indexAClient.GetIndexParametersAsync();
            var indexCParameters = await indexCClient.GetIndexParametersAsync();

            // Assert.
            //each client calls its own index over the shared http client, and gets its own answer
            Assert.Equal(
                [
                    new Uri("https://index-b.example/api/v0.3/system/parameters"),
                    new Uri("https://index-a.example/api/v0.3/system/parameters"),
                    new Uri("https://index-c.example/api/v0.3/system/parameters")
                ],
                indexHandler.RequestUris);
            Assert.Equal(IndexHandler.GetCommentMaxLength("index-a.example"), indexAParameters.CommentMaxLength);
            Assert.Equal(IndexHandler.GetCommentMaxLength("index-b.example"), indexBParameters.CommentMaxLength);
            Assert.Equal(IndexHandler.GetCommentMaxLength("index-c.example"), indexCParameters.CommentMaxLength);
        }

        [Fact]
        public void AddEthernaIndexClient_RegistersAFactoryRefusingRelativeIndexUrls()
        {
            // Setup.
            using var serviceProvider = BuildServiceProvider(new IndexHandler());
            var indexClientFactory = serviceProvider.GetRequiredService<IEthernaUserIndexClientFactory>();

            // Action and assert.
            Assert.Throws<ArgumentException>(() => indexClientFactory.Create(new Uri("index-b", UriKind.Relative)));
        }

        // Helpers.
        private static ServiceProvider BuildServiceProvider(IndexHandler indexHandler)
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<ISwarmClient>());
            services.AddEthernaUserClients(
                    clientId: "clientId",
                    clientSecret: null,
                    returnUrlPort: 3000)
                .AddEthernaIndexClient();
            services.AddHttpClient(ServiceCollectionExtensions.DefaultEthernaUserHttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() => indexHandler);
            return services.BuildServiceProvider();
        }
    }
}
